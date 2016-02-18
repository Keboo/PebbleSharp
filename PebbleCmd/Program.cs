
using System;
using System.Linq;
using System.Threading.Tasks;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;
using System.IO;
using PebbleSharp.Net45;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.AppMessage;

namespace PebbleCmd
{
    /// <summary>
    /// A simple console application for testing messages with the Pebble watch.
    /// </summary>
    class Program
    {
        static void Main()
        {
            ShowPebbles().Wait();
        }

        private static async Task ShowPebbles()
        {
            Console.WriteLine( "PebbleCmd" );
            Console.WriteLine( "Select Pebble to connect to:" );
            var pebbles = PebbleNet45.DetectPebbles();
            var options = pebbles.Select( x => x.PebbleID ).Union( new[] { "Exit" } );
            var menu = new Menu( options.ToArray() );
            var result = menu.ShowMenu();
            if ( result >= 0 && result < pebbles.Count )
            {
                var selectedPebble = pebbles[result];
                selectedPebble.RegisterCallback<AppMessagePacket>(ReceiveAppMessage);
                Console.WriteLine( "Connecting to Pebble " + selectedPebble.PebbleID );
                await selectedPebble.ConnectAsync();
                Console.WriteLine( "Connected" );
                await ShowPebbleMenu( selectedPebble );
            }
        }

        private static async Task ShowPebbleMenu( Pebble pebble )
        {
            var menu = new Menu(
                "Disconnect",
                "Get Time",
                "Set Current Time",
                "Get Firmware Info",
                "Send Ping",
                "Media Commands",
                "Install App",
                "Send App Message");
            while ( true )
            {
                switch ( menu.ShowMenu() )
                {
                    case 0:
                        pebble.Disconnect();
                        return;
                    case 1:
                        var timeResult = await pebble.GetTimeAsync();
                        DisplayResult( timeResult, x => string.Format( "Pebble Time: " + x.Time.ToString( "G" ) ) );
                        break;
                    case 2:
                        await pebble.SetTimeAsync( DateTime.Now );
                        goto case 1;
                    case 3:
                        var firmwareResult = await pebble.GetFirmwareVersionAsync();
                        DisplayResult( firmwareResult,
                            x => string.Join( Environment.NewLine, "Firmware", x.Firmware.ToString(),
                                "Recovery Firmware", x.RecoveryFirmware.ToString() ) );
                        break;
                    case 4:
                        var pingResult = await pebble.PingAsync();
                        DisplayResult( pingResult, x => "Received Ping Response" );
                        break;
                    case 5:
                        ShowMediaCommands( pebble );
                        break;
                    case 6:
                        InstallApp(pebble);
                        break;
                    case 7:
                        SendAppMessage(pebble);
                        break;
                }
            }
        }

        private static string SelectApp()
        {
			string exePath = System.Reflection.Assembly.GetExecutingAssembly ().CodeBase;
            //TODO: there has to be a better way to come up with a canonical path, but this combo seems to work on both windows and 'nix
            if (exePath.StartsWith("file:"))
            {
                exePath = exePath.Substring(5);
            }
            if (exePath.StartsWith("///"))
            {
                exePath = exePath.Substring(3);
            }
			string exeDir = Path.GetDirectoryName (exePath);
			var dir = new DirectoryInfo (exeDir);
			var files = dir.GetFiles ("*.pbw");

            if (files.Any())
            {
                if (files.Count() == 1)
                {
                    return files.Single().FullName;
                }
                else
                {
                    var fileMenu = new Menu(files.Select(x => x.Name).ToArray());
                    int index = fileMenu.ShowMenu();
                    return files[index].FullName;
                }
            }
            else
            {
                Console.WriteLine("No .pbw files found");
                return null;
            }
        }

        private static void InstallApp(Pebble pebble)
        {
            var progress = new Progress<ProgressValue>(pv => Console.WriteLine(pv.ProgressPercentage + " " + pv.Message));

            string appPath = SelectApp();

            if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
            {
                using (var stream = new FileStream(appPath, FileMode.Open))
                {
                    using (var zip = new Zip())
                    {
                        zip.Open(stream);
                        var bundle = new AppBundle();
                        stream.Position = 0;
						bundle.Load(zip,pebble.Firmware.HardwarePlatform.GetSoftwarePlatform());
                        pebble.InstallClient.InstallAppAsync(bundle, progress).Wait();
                        
						//for firmware v3, launch is done as part of the install
                        //Console.WriteLine("App Installed, launching...");
						//var uuid=new UUID(bundle.AppInfo.UUID);
						//pebble.LaunchApp(uuid);
						//Console.WriteLine ("Launched");
                    }
                }
            }
            else
            {
                Console.WriteLine("No .pbw");
            }
        }

        private static void SendAppMessage(Pebble pebble)
        {
            string uuidAppPath = SelectApp();

            if (!string.IsNullOrEmpty(uuidAppPath) && File.Exists(uuidAppPath))
            {
			    using (var stream = new FileStream(uuidAppPath, FileMode.Open))
			    {
				    using (var zip = new Zip())
				    {
					    zip.Open(stream);
					    var bundle = new AppBundle();
					    stream.Position = 0;
					    bundle.Load(zip,pebble.Firmware.HardwarePlatform.GetSoftwarePlatform());

                        System.Console.Write("Enter Message:");
                        var messageText = System.Console.ReadLine();

					    //format a message
					    var rand = new Random().Next();
					    AppMessagePacket message = new AppMessagePacket();
                        message.Command = (byte)Command.Push;
					    message.Values.Add(new AppMessageUInt32() { Key=0,Value = (uint)rand });
					    message.Values.Add(new AppMessageString() { Key=1,Value = messageText });
					    message.ApplicationId = bundle.AppMetadata.UUID;
					    message.TransactionId = 255;


					    //send it
					    Console.WriteLine("Sending Status "+rand+" to " + bundle.AppMetadata.UUID.ToString());
                        var task = pebble.SendApplicationMessage(message);
                        task.Wait();
					    Console.WriteLine("Response received");
				    }
			    }



            }
            else
            {
                Console.WriteLine("No .pbw");
            }
        }

        private static void ShowMediaCommands( Pebble pebble )
        {
            Console.WriteLine( "Listening for media commands" );
            pebble.RegisterCallback<MusicControlResponse>( result =>
                DisplayResult( result, x => string.Format( "Media Control Response " + x.Command ) ) );

            var menu = new Menu( "Return to menu" );

            while ( true )
            {
                switch ( menu.ShowMenu() )
                {
                    case 0:
                        return;
                }
            }
        }

        private static void DisplayResult<T>( T result, Func<T, string> successData )
            where T : ResponseBase
        {
            if ( result.Success )
            {
                Console.WriteLine( successData( result ) );
            }
            else
            {
                Console.WriteLine( "ERROR" );
                Console.WriteLine( result.ErrorMessage );
                Console.WriteLine( result.ErrorDetails.ToString() );
            }
        }

        private static void ReceiveAppMessage(AppMessagePacket response)
        {
            System.Console.WriteLine("Recieved App Message");
        }
    }
}
