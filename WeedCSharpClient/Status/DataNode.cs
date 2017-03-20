namespace WeedCSharpClient.Status
{
    public class DataNode : AbstractNode
    {
        public string PublicUrl { get; set; }
        public string Url { get; set; }
        public int Volumes { get; set; }

        public Location AsLocation()
        {
            return new Location
                        {
                            publicUrl = PublicUrl,
                            url = Url
                        };
        }
    }
}
