namespace Porganizer
{
    public class Series : BindableBase
    {
        public Series()
        {
        }

        // Attributes.
        private int seriesId;
        public int SeriesId
        {
            get { return this.seriesId; }
            set { this.SetProperty(ref this.seriesId, value); }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}
