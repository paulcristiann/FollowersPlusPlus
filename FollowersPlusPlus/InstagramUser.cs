namespace FollowersPlusPlus
{
    public class InstagramUser
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public long UserId { get; set; }
        public override string ToString()
        {
            return FullName + "(" + Username + ")";
        }
    }
}
