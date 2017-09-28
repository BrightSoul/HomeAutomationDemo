using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using HomeAutomationDemo.Web.Extensions;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class ConsoleFacility : IDeviceControlFacility
    {
        public event EventHandler<Command> CommandReceived;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IDeviceStatusProvider deviceStatusProvider;
        private readonly IApplicationLifetime applicationLifetime;

        public ConsoleFacility(IDeviceStatusProvider deviceStatusProvider, IApplicationLifetime applicationLifetime)
        {
            Console.CancelKeyPress += TerminateApplication;
            this.applicationLifetime = applicationLifetime;
            this.deviceStatusProvider = deviceStatusProvider;
            cancellationTokenSource = new CancellationTokenSource();
            ReadCommands();
        }

        private void TerminateApplication(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            applicationLifetime.StopApplication();
        }

        private async void ReadCommands()
        {
            //Give time to kestrel to output all its stuff to the console
            await Task.Delay(1000);

            Console.WriteLine("Console facility started. Type help to see a command list.");
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            try
            {
                while (true)
                {
                    //Console.Write("> ");
                    var input = (await Console.In.ReadLineAsync()).AsCommandArguments();
                    var commandName = input.FirstOrDefault()?.ToLowerInvariant();
                    switch (commandName)
                    {
                        case "help":
                            PrintInstructions();
                            break;
                        case "light":
                            HandleLightCommand(input);
                            break;
                        case "alarm":
                            HandleAlarmCommand(input);
                            break;
                        case "doorbell":
                            await HandleDoorbellCommand(input);
                            break;
                        case "":
                        case null:
                            break;
                        default:
                            Console.WriteLine($"Invalid command \"{commandName}\"");
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task HandleDoorbellCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 1)
            {
                return;
            }

            CommandReceived?.Invoke(this, new UpdateDoorbell { DesiredStatus = DoorbellStatus.On });
            await Task.Delay(3000);
            CommandReceived?.Invoke(this, new UpdateDoorbell { DesiredStatus = DoorbellStatus.Off });
        }

        private void HandleAlarmCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 2)
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[1], true, out AlarmStatus alarmStatus))
            {
                return;
            }

            CommandReceived?.Invoke(this, new UpdateAlarm { DesiredStatus = alarmStatus });
        }

        private void HandleLightCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 3)
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[1], true, out LightStatus lightStatus))
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[2], true, out Light light))
            {
                return;
            }

            CommandReceived?.Invoke(this, new UpdateLight { Light = light, DesiredStatus = lightStatus });
        }

        public Task UpdateAlarm(AlarmStatus status)
        {
            Console.WriteLine($"The alarm is now {status.ToString().ToUpperInvariant()}");
            return Task.CompletedTask;
        }

        public Task UpdateLight(Light light, LightStatus status)
        {
            Console.WriteLine($"The light in the {light.ToString().ToUpperInvariant()} room is now {status.ToString().ToUpperInvariant()}");
            return Task.CompletedTask;
        }

        public Task UpdateDoorbell(DoorbellStatus status)
        {
            Console.WriteLine($"The doorbell is now {status.ToString().ToUpperInvariant()}");
            return Task.CompletedTask;
        }

        private void PrintInstructions()
        {
            Console.WriteLine("Here's the list of available commands:");
            Console.WriteLine("\tLIGHT <ON|OFF> <RED|YELLOW|BLUE|GREEN>");
            Console.WriteLine("\tALARM <ON|OFF|ACTIVE>");
            Console.WriteLine("\tDOORBELL");
        }

        public void Dispose()
        {
            Console.CancelKeyPress -= TerminateApplication;
            cancellationTokenSource.Cancel();
        }
    }
}
