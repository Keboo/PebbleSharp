using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using PebbleSharp.Core.Responses;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.BlobDB;

namespace PebbleSharp.Core.Install
{
	public class InstallClient
	{
		private Pebble _pebble;
		public InstallClient(Pebble pebble)
		{
			_pebble = pebble;
		}
		public async Task InstallAppAsync( AppBundle bundle, IProgress<ProgressValue> progress = null )
		{

			string version = _pebble.Firmware.Version;
			version = version.Replace("v", "");
			var components = version.Split(new char[] { '.','-' }, StringSplitOptions.RemoveEmptyEntries);

			int i;
			IList<int> versionComponents = components.Where(x=>int.TryParse(x,out i)).Select(x => int.Parse(x)).ToList();
			if (versionComponents[0] < 3) 
			{
				await InstallAppLegacyV2 (bundle, progress);
			} 
			else 
			{
				await InstallAppAsyncV3 (bundle, progress);
			}
		}


		private async Task InstallAppAsyncV3(AppBundle bundle,IProgress<ProgressValue> progress)
		{
			//https://github.com/pebble/libpebble2/blob/master/libpebble2/services/install.py

			var meta = AppMetaData.FromAppBundle(bundle);

			var bytes = meta.GetBytes();
			var result = await _pebble.BlobDBClient.Delete(BlobDatabase.App, meta.UUID.Data);

			result = await _pebble.BlobDBClient.Insert(BlobDatabase.App, meta.UUID.Data, bytes);

			if (result.Response == BlobStatus.Success)
			{
				var startPacket = new AppRunStatePacket();
				startPacket.Command = AppRunState.Start;
				startPacket.UUID = meta.UUID;
				//app_fetch = self._pebble.send_and_read(AppRunState(data=AppRunStateStart(uuid=app_uuid)), AppFetchRequest)

				var runStateResult = await _pebble.SendMessageAsync<AppFetchRequestPacket>(Endpoint.AppRunState, startPacket.GetBytes());

				if (!runStateResult.Success)
				{
					throw new InvalidOperationException("Pebble replied invalid run state");
				}

				if (!meta.UUID.Equals(runStateResult.UUID))
				{
					var response = new AppFetchResponsePacket();
					response.Response = AppFetchStatus.InvalidUUID;
					await _pebble.SendMessageNoResponseAsync(Endpoint.AppFetch, response.GetBytes());
					throw new InvalidOperationException("The pebble requested the wrong UUID");
				}

				var putBytesResponse = await _pebble.PutBytesClient.PutBytes(bundle.App, TransferType.Binary, appInstallId:(uint)runStateResult.AppId);
				if (!putBytesResponse)
				{
					throw new InvalidOperationException("Putbytes failed");
				}

				if (bundle.HasResources)
				{
					putBytesResponse = await _pebble.PutBytesClient.PutBytes(bundle.Resources, TransferType.Resources, appInstallId:(uint)runStateResult.AppId);
					if (!putBytesResponse)
					{
						throw new InvalidOperationException("Putbytes failed");
					}
				}

				//TODO: add worker to manifest and transfer it if necassary
				//if (bundle.HasWorker)
				//{
				//await PutBytesV3(bundle.Worker, TransferType.Worker, runStateResult.AppId);
				//}

			}
			else 
			{
				throw new DataMisalignedException("BlobDB Insert Failed");
			}
		}

		private async Task InstallAppLegacyV2(AppBundle bundle, IProgress<ProgressValue> progress= null)
		{
			if ( bundle == null )
				throw new ArgumentNullException( "bundle" );

			if ( progress != null )
				progress.Report( new ProgressValue( "Removing previous install(s) of the app if they exist", 1 ) );
			ApplicationMetadata metaData = bundle.AppMetadata;
			UUID uuid = metaData.UUID;

			AppbankInstallResponse appbankInstallResponse = await RemoveAppByUUID( uuid );
			if ( appbankInstallResponse.Success == false )
				return;

			if ( progress != null )
				progress.Report( new ProgressValue( "Getting current apps", 20 ) );
			AppbankResponse appBankResult = await GetAppbankContentsAsync();

			if ( appBankResult.Success == false )
				throw new PebbleException( "Could not obtain app list; try again" );
			AppBank appBank = appBankResult.AppBank;

			byte firstFreeIndex = 1;
			foreach ( App app in appBank.Apps )
				if ( app.Index == firstFreeIndex )
					firstFreeIndex++;
			if ( firstFreeIndex == appBank.Size )
				throw new PebbleException( "All app banks are full" );

			if ( progress != null )
				progress.Report( new ProgressValue( "Transferring app to Pebble", 40 ) );

			if ( await _pebble.PutBytesClient.PutBytes( bundle.App, TransferType.Binary,index:firstFreeIndex ) == false )
				throw new PebbleException( "Failed to send application binary pebble-app.bin" );

			if ( bundle.HasResources )
			{
				if ( progress != null )
					progress.Report( new ProgressValue( "Transferring app resources to Pebble", 60 ) );
				if ( await _pebble.PutBytesClient.PutBytes( bundle.Resources, TransferType.Resources,index:firstFreeIndex ) == false )
					throw new PebbleException( "Failed to send application resources app_resources.pbpack" );
			}

			if ( progress != null )
				progress.Report( new ProgressValue( "Adding app", 80 ) );
			await AddApp( firstFreeIndex );
			if ( progress != null )
				progress.Report( new ProgressValue( "Done", 100 ) );
		}



		public async Task<bool> InstallFirmwareAsync( FirmwareBundle bundle, IProgress<ProgressValue> progress = null )
		{
			if ( bundle == null ) throw new ArgumentNullException( "bundle" );

			if ( progress != null )
				progress.Report( new ProgressValue( "Starting firmware install", 1 ) );
			if ( ( await _pebble.SendSystemMessageAsync( SystemMessage.FirmwareStart ) ).Success == false )
			{
				return false;
			}

			if ( bundle.HasResources )
			{
				if ( progress != null )
					progress.Report( new ProgressValue( "Transfering firmware resources", 25 ) );
				if ( await _pebble.PutBytesClient.PutBytes( bundle.Resources, TransferType.SysResources,index:0 ) == false )
				{
					return false;
				}
			}

			if ( progress != null )
				progress.Report( new ProgressValue( "Transfering firmware", 50 ) );
			if ( await _pebble.PutBytesClient.PutBytes( bundle.Firmware, TransferType.Firmware,index:0 ) == false )
			{
				return false;
			}

			if ( progress != null )
				progress.Report( new ProgressValue( "Completing firmware install", 75 ) );
			bool success = ( await _pebble.SendSystemMessageAsync( SystemMessage.FirmwareComplete ) ).Success;

			if ( progress != null )
				progress.Report( new ProgressValue( "Done installing firmware", 100 ) );

			return success;
		}

		public async Task<AppbankInstallResponse> RemoveAppByUUID( UUID uuid )
		{
			byte[] data = Util.CombineArrays( new byte[] { 2 }, uuid.Data );
			return await _pebble.SendMessageAsync<AppbankInstallResponse>( Endpoint.AppManager, data );
		}

		public async Task AddApp( byte index )
		{
			byte[] data = Util.CombineArrays( new byte[] { 3 }, Util.GetBytes( (uint)index ) );
			await _pebble.SendMessageNoResponseAsync( Endpoint.AppManager, data );
		}

		public async Task<AppbankResponse> GetAppbankContentsAsync()
		{
			return await _pebble.SendMessageAsync<AppbankResponse>( Endpoint.AppManager, new byte[] { 1 } );
		}
	}
}

