using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Autofac;

namespace BotDebugger
{
    class Program
    {
        static void Main(string[] args)
        {

        }
        static async Task Interactive<T>(IDialog<T> form) where T : class
        {

            Console.OutputEncoding = Encoding.GetEncoding(65001);
            var message = new Activity()
            {
                From = new ChannelAccount { Id = "ConsoleUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = "FormTest" },
                ChannelId = "Console",
                ServiceUrl = "http://localhost:9000/",
                Text = ""
            };
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());
            builder.RegisterType<InMemoryDataStore>()
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .SingleInstance();
            builder
                .Register(c => new BotIdResolver("testBot"))
                    .As<IBotIdResolver>()
                    .SingleInstance();
            builder
                .Register(c => new BotToUserTextWriter(new BotToUserQueue(message, new Queue<IMessageActivity>()), Console.Out))
                    .As<IBotToUser>()
                    .InstancePerLifetimeScope();
            using (var container = builder.Build())
            using (var scope = DialogModule.BeginLifetimeScope(container, message))
            {
                Func<IDialog<object>> MakeRoot = () => form;
                DialogModule_MakeRoot.Register(scope, MakeRoot);
                var task = scope.Resolve<IPostToBot>();
                await scope.Resolve<IBotData>().LoadAsync(default(CancellationToken));
                var stack = scope.Resolve<IDialogStack>();
                stack.Call(MakeRoot(), null);
                await stack.PollAsync(CancellationToken.None);
                while (true)
                {
                    message.Text = await Console.In.ReadLineAsync();
                    //message.Locale = Locale;
                    await task.PostAsync(message, CancellationToken.None);
                }

            }
        }
    }
}
