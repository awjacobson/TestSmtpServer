using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestSmtpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25, 587)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SampleMessageStore());

            var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
            await smtpServer.StartAsync(CancellationToken.None);
        }
    }

    internal class SampleMessageStore : MessageStore
    {
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }

            stream.Position = 0;

            var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
            Console.WriteLine(message.Attachments);
            Console.WriteLine(message.TextBody);

            return SmtpResponse.Ok;
        }
    }
}
