using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;

namespace DYAlertViewPicker
{
	public class DyAlertViewTask
	{
        //Only use to get one the main ui-thread
		private readonly NSObject _obj = new NSObject();

        /// <summary>
        /// Throw an exception if user canceled DYAlertPickView
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="confirm"></param>
        /// <param name="items"></param>
        /// <returns></returns>
		public Task<string> GetResult(string title, string cancel, string confirm, List<string> items)
		{

			var task = new TaskCompletionSource<string>();
		    var alert = new DyAlertPickerView(title, cancel, confirm, string.Empty)
		    {
		        ItemList = items.AsReadOnly(),
		        TapBackgroundToDismiss = true
		    };
		    alert.OnConfirm += (s, value) => task.SetResult(value);
            // This will throw a exception instead of returning null
			alert.OnCancel += (sender, args) => task.SetCanceled();
            _obj.InvokeOnMainThread(()=>alert.Show());

			return task.Task;
		}

        /// <summary>
        /// Return 'null' when users canceled
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="confirm"></param>
        /// <param name="items"></param>
        /// <returns></returns>
	    public Task<string> TryGetResult(string title, string cancel, string confirm, List<string> items)
	    {
	        var task = new TaskCompletionSource<string>();
	        var alert = new DyAlertPickerView(title, cancel, confirm, string.Empty)
	        {
	            ItemList = items.AsReadOnly(),
	            TapBackgroundToDismiss = true
	        };
	        alert.OnConfirm += (s, value) => task.SetResult(value);
	        alert.OnCancel += (sender, args) => task.SetResult(null);
	        _obj.InvokeOnMainThread(() => alert.Show());

	        return task.Task;
	    }


    }
}

