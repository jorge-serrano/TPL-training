namespace AsyncWinFormsApp
{
    public class AsyncConstructors
    {
        private AsyncConstructors()
        {

        }
        private async Task<AsyncConstructors> InitAsyncConstructorsAsync() {
            await Task.Delay(100);//long running operation
            return this;
        }
        public static async Task<AsyncConstructors> CreateAsync(){
           var t = new AsyncConstructors();
            return await t.InitAsyncConstructorsAsync();
            }
    }
}
