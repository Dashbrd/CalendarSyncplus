namespace Test.Model
{
    public class Calendar : System.Waf.Foundation.Model
    {
        private string _name;
        private string _id;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
    }
}