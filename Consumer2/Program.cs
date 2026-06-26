using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configuration;
using System.Text;

namespace Consumer2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // إعداد وقراءة ملف الإعدادات appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var rabbitMQConfig = configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQConfig.Server,
                UserName = rabbitMQConfig.UserName,
                Password = rabbitMQConfig.Password
            };

            // 1. إنشاء الاتصال والقناة بشكل غير متزامن (Async)
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // 2. الإعلان عن الـ Fanout Exchange لضمان وجوده سواء اشتغل البروديوسر أو الكونسومر الأول
            await channel.ExchangeDeclareAsync(rabbitMQConfig.ExchangeName, type: ExchangeType.Direct);

            // 3. إنشاء طابور مؤقت ومجهول الاسم (Temporary / Anonymous Queue)
            // هذا الطابور خاص بنسخة التطبيق الحالية وسيتم حذفه تلقائياً بمجرد إغلاق الاتصال
            var queue = await channel.QueueDeclareAsync();
            var queueName = queue.QueueName;

            // 4. ربط الطابور المؤقت بالـ Fanout Exchange (تصحيح خطأ الـ ExchangeName)
            // تم تعديل الـ exchange ليأخذ ExchangeName بدلاً من QueueName ليعمل التوجيه بشكل صحيح
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: rabbitMQConfig.ExchangeName, // تم الإصلاح هنا
                routingKey: rabbitMQConfig.Bindingkey);

            // ملحوظة: تم حذف سطر الـ BasicQosAsync (Prefetch Count) لأنه غير مستخدم في نمط الـ Pub/Sub بناءً على الفيديو

            // 5. إنشاء الـ Consumer المخصص للتعامل مع الـ Async
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Second Consumer [x] Received message: {message}");

                // هنا يمكنك تنفيذ العمليات الطويلة (مثل الحفظ في قاعدة البيانات) باستخدام await
                await Task.CompletedTask;

                // 6. إرسال تأكيد الاستلام يدوياً (Manual Acknowledgment) بعد انتهاء المعالجة تماماً
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            // 7. بدء استهلاك الرسائل من الطابور المؤقت مع تفعيل الـ Manual Ack (autoAck: false)
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false, // تعني أننا سنرسل الـ Ack يدوياً عبر الكود بالأسفل
                consumer: consumer);

            Console.WriteLine(" [*] Waiting for messages. Press [enter] to exit.");

            // منع تطبيق الـ Console من الإغلاق فوراً
            Console.ReadLine();
        }
    }
}