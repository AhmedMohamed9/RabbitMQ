using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configuration;
using System.Text;

namespace Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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

            // 1. إنشاء الاتصال والقناة بشكل Async
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // 2. الإعلان عن الطابور بشكل Async
            await channel.QueueDeclareAsync(
                queue: rabbitMQConfig.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            // 3. إنشاء الـ Consumer المخصص للـ Async
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[x] Received message: {message}");

                // بما أنك تستخدم autoAck: true، السيرفر سيحذف الرسالة تلقائياً بمجرد وصولها هنا.
                // إذا قمت بعمليات طويلة هنا (مثل حفظ في قاعدة البيانات)، يمكنك استخدام await.
                await Task.CompletedTask; // نرجع Task مكتمل لإرضاء توقيع الدالة

                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

            };

            // 4. استدعاء دالة الاستهلاك الحديثة باستخدام await و Async
            await channel.BasicConsumeAsync(
                queue: rabbitMQConfig.QueueName,
                autoAck: false, // تأكيد الاستلام التلقائي
                consumer: consumer);

            Console.WriteLine(" [*] Waiting for messages. Press [enter] to exit.");

            // 5. منع تطبيق الـ Console من الإغلاق فوراً
            Console.ReadLine();
        }
    }
}