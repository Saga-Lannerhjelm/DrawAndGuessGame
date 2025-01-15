namespace webbAPI.Models.ViewModels
{
    public class UserVM
    {
        public User Info { get; set; } = new User();
        public UserInRound Round { get; set; } = new UserInRound();

        public int TotalRoundPoints { get; set; }
    }
}