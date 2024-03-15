namespace rose
{
    [Serializable]
    public struct RdpRequest
    {
        public string DisplayName { get; set; }
        public string Command { get; set; }
        public string Arguements {get; set; }
    }
}