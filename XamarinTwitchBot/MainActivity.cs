// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Main activity
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;

    [Activity(Label = "TwitchBot", MainLauncher = true, Icon = "@drawable/icon",
     ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.RequestWindowFeature(WindowFeatures.ActionBar);
            var actionBar = this.ActionBar;
            actionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            actionBar.SetDisplayShowTitleEnabled(false);

            this.SetContentView(Resource.Layout.Main);

            var inviter = new Fragments.InviterFragment();
            var irc = new Fragments.IrcFragment();

            var inviterTab = actionBar.NewTab();
            inviterTab.SetText("INVITER");
            inviterTab.TabSelected += (sender, e) =>
            {
                if (!inviter.IsAdded && !inviter.IsHidden)
                {
                    e.FragmentTransaction.Add(Resource.Id.fragmentContainer, inviter, "Inviter"); // Android.Resource.Id.Content
                }
                else
                {
                    e.FragmentTransaction.Show(inviter);
                }
            };
            inviterTab.TabUnselected += (sender, e) =>
            {
                e.FragmentTransaction.Hide(inviter);
            };
            actionBar.AddTab(inviterTab);

            actionBar.SelectTab(inviterTab);

            var ircTab = actionBar.NewTab();
            ircTab.SetText("IRC BOT");
            ircTab.TabSelected += (sender, e) =>
            {
                if (!irc.IsAdded && !irc.IsHidden)
                {
                    e.FragmentTransaction.Add(Resource.Id.fragmentContainer, irc, "Irc");
                }
                else
                {
                    e.FragmentTransaction.Show(irc);
                }
            };
            ircTab.TabUnselected += (sender, e) =>
            {
                e.FragmentTransaction.Hide(irc);
            };
            actionBar.AddTab(ircTab);
        }
    }
}
