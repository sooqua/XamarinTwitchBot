// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Irc fragment
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Fragments
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Android.App;
    using Android.OS;
    using Android.Views;
    using Android.Widget;

    using Common;
    using Common.Exceptions;

    using Newtonsoft.Json;

    using XamarinTwitchBot.Irc;

    public class IrcFragment : Fragment
    {
        private IrcBot ircBot = new IrcBot();
        private CancellationTokenSource cts = new CancellationTokenSource();
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // return base.OnCreateView(inflater, container, savedInstanceState);
            var linearLayout = (LinearLayout)inflater.Inflate(Resource.Layout.Irc, container, false);

            var ui = new UI(linearLayout);

            ui.LoginBtn.Click += async delegate
            {
                try
                {
                    ui.PendingAuthorization();
                    await this.ircBot.LogInAsync(ui.UsernameTBox.Text, ui.PasswordTBox.Text);
                    await this.ircBot.Moderator.LogInAsync(ui.ModUsernameTBox.Text, ui.ModPasswordTBox.Text);
                    if (!string.IsNullOrWhiteSpace(await this.ircBot.GetOAuthAsync()) && !string.IsNullOrWhiteSpace(await this.ircBot.Moderator.GetOAuthAsync()))
                    {
                        ui.Authorized();
                    }
                    else
                    {
                        Utils.LogError("Couldn't get OAuth key");
                        ui.FailedAuthorization();
                    }
                }
                catch (HttpRequestException ex)
                {
                    Utils.LogError("Something went wrong. They probably changed the algorithm.\n" + ex);
                    ui.FailedAuthorization();
                }
                catch (WebException ex)
                {
                    Utils.LogError("Check your internet connection.\n" + ex);
                    ui.FailedAuthorization();
                }
            };
            
            ui.StartBotBtn.Click += async delegate
            {
                Utils.LogDebug("Someone clicked the StartBotBtn");
                if (ui.IsIdle())
                {
                    Utils.LogDebug("UI is idle. ui.StartBotBtn.Text = " + ui.StartBotBtn.Text);
                    ui.InProgress();

                    while (true)
                    {
                        Utils.LogDebug("New iteration before ConnectAndStartListeningAsync has occurred");
                        try
                        {
                            await this.ircBot.ConnectAndStartListeningAsync(ui.UsernameTBox.Text, ui.ModUsernameTBox.Text, ui.GroupNameTBox.Text, this.cts.Token);
                        }
                        catch (System.OperationCanceledException)
                        {
                            if (this.cts.Token.IsCancellationRequested)
                            {
                                Utils.LogInfo("Task cancelled");
                                ui.Finished();
                                this.cts = new CancellationTokenSource();
                            }
                            else
                            {
                                // probably SendAsync() timeout
                                Utils.LogError("Operation canceled unexpectedly. IrcBot is going to be re-created");
                                this.ircBot = new IrcBot();
                            }

                            break;
                        }
                        catch (OAuthIsNullException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (JsonException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (HttpRequestException ex)
                        {
                            Utils.LogError("Something went wrong. They probably changed the algorithm.\n" + ex);
                        }
                        catch (WebException ex)
                        {
                            Utils.LogError("Check your internet connection.\n" + ex);
                        }
                        catch (ArgumentNullException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (SocketException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError("SOME WEIRD SHIT JUST HAPPENED, ANALYZE YOUR DAMN PROGRAM");
                            Utils.LogWtf(ex.ToString());
                        }

                        // Wait a delay before trying to reconnect
                        await Task.Delay(3000);
                    }
                }
                else
                {
                    this.cts.Cancel();
                }
            };

            ui.LogoutBtn.Click += delegate
            {
                var confirmDialog = new AlertDialog.Builder(this.Activity);
                confirmDialog.SetMessage("Do you really want to log out?");
                confirmDialog.SetNeutralButton("Yes", delegate
                                                    {
                                                        this.ircBot.DeleteCookies();
                                                        this.ircBot = new IrcBot();
                                                        ui.UnAuthorized();
                                                    });
                confirmDialog.SetNegativeButton("No", delegate { });
                confirmDialog.Show();
            };

            if (this.ircBot.IsAuthenticated())
            {
                ui.PendingAuthorization();
                Task.Run(async () =>
                {
                    try
                    {
                        await this.ircBot.GetOAuthAsync();
                        await this.ircBot.Moderator.GetOAuthAsync();
                        if (!string.IsNullOrWhiteSpace(this.ircBot.OAuth) &&
                            !string.IsNullOrWhiteSpace(this.ircBot.Moderator.OAuth))
                        {
                            Utils.InvokeOnMainThread(() => { ui.Authorized(); });
                            return;
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Utils.LogError("Something went wrong. They probably changed the algorithm.\n" + ex);
                    }
                    catch (WebException ex)
                    {
                        Utils.LogError("Check your internet connection.\n" + ex);
                    }

                    Utils.LogError("Couldn't get OAuth key");
                    Utils.InvokeOnMainThread(() => { ui.FailedAuthorization(); });
                });
            }

            return linearLayout;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "All UI controls should be accessible.")]

        // ReSharper disable once InconsistentNaming
        public class UI
        {
            public readonly TextView LabelUsername;
            public readonly EditText UsernameTBox;
            public readonly TextView LabelPassword;
            public readonly EditText PasswordTBox;

            public readonly TextView LabelModUsername;
            public readonly EditText ModUsernameTBox;
            public readonly TextView LabelModPassword;
            public readonly EditText ModPasswordTBox;

            public readonly Button LoginBtn;

            public readonly TextView LabelGroupName;
            public readonly EditText GroupNameTBox;

            public readonly Button StartBotBtn;

            public readonly Button LogoutBtn;

            public UI(View view)
            {
                this.LabelUsername = view.FindViewById<TextView>(Resource.Id.IRClabelUsername);
                this.UsernameTBox = view.FindViewById<EditText>(Resource.Id.IRCusernameTBox);
                this.LabelPassword = view.FindViewById<TextView>(Resource.Id.IRClabelPassword);
                this.PasswordTBox = view.FindViewById<EditText>(Resource.Id.IRCpasswordTBox);

                this.LabelModUsername = view.FindViewById<TextView>(Resource.Id.IRClabelmodUsername);
                this.ModUsernameTBox = view.FindViewById<EditText>(Resource.Id.IRCmodusernameTBox);
                this.LabelModPassword = view.FindViewById<TextView>(Resource.Id.IRClabelmodPassword);
                this.ModPasswordTBox = view.FindViewById<EditText>(Resource.Id.IRCmodpasswordTBox);

                this.LoginBtn = view.FindViewById<Button>(Resource.Id.IRCloginBtn);

                this.LabelGroupName = view.FindViewById<TextView>(Resource.Id.IRClabelGroupName);
                this.GroupNameTBox = view.FindViewById<EditText>(Resource.Id.IRCgroupNameTBox);

                this.StartBotBtn = view.FindViewById<Button>(Resource.Id.IRCstartBotBtn);

                this.LogoutBtn = view.FindViewById<Button>(Resource.Id.IRClogoutBtn);

                this.UsernameTBox.Text = SharedPreferences.GetString("USERNAME", "IRCUSERNAME");
                this.PasswordTBox.Text = SharedPreferences.GetString("PASSWORD", "IRCPASSWORD");
                this.ModUsernameTBox.Text = SharedPreferences.GetString("MODUSERNAME", "IRCMODUSERNAME");
                this.ModPasswordTBox.Text = SharedPreferences.GetString("MODPASSWORD", "IRCMODPASSWORD");
                this.GroupNameTBox.Text = SharedPreferences.GetString("GROUPNAME", "GROUPNAME");
            }

            public void UnAuthorized()
            {
                this.LogoutBtn.Visibility = ViewStates.Gone;

                this.StartBotBtn.Visibility = ViewStates.Gone;

                this.GroupNameTBox.Visibility = ViewStates.Gone;
                this.LabelGroupName.Visibility = ViewStates.Gone;

                this.LabelUsername.Visibility = ViewStates.Visible;
                this.UsernameTBox.Visibility = ViewStates.Visible;
                this.LabelPassword.Visibility = ViewStates.Visible;
                this.PasswordTBox.Visibility = ViewStates.Visible;

                this.LabelModUsername.Visibility = ViewStates.Visible;
                this.ModUsernameTBox.Visibility = ViewStates.Visible;
                this.LabelModPassword.Visibility = ViewStates.Visible;
                this.ModPasswordTBox.Visibility = ViewStates.Visible;

                this.LoginBtn.Visibility = ViewStates.Visible;

                this.UsernameTBox.Enabled = true;
                this.PasswordTBox.Enabled = true;
                this.ModUsernameTBox.Enabled = true;
                this.ModPasswordTBox.Enabled = true;
                this.LoginBtn.Enabled = true;
            }

            public void PendingAuthorization()
            {
                this.LoginBtn.Enabled = false;
                this.UsernameTBox.Enabled = false;
                this.PasswordTBox.Enabled = false;
                this.ModUsernameTBox.Enabled = false;
                this.ModPasswordTBox.Enabled = false;

                SharedPreferences.PutString("USERNAME", this.UsernameTBox.Text, "IRCUSERNAME");
                SharedPreferences.PutString("PASSWORD", this.PasswordTBox.Text, "IRCPASSWORD");
                SharedPreferences.PutString("MODUSERNAME", this.ModUsernameTBox.Text, "IRCMODUSERNAME");
                SharedPreferences.PutString("MODPASSWORD", this.ModPasswordTBox.Text, "IRCMODPASSWORD");
            }

            public void FailedAuthorization()
            {
                this.LoginBtn.Enabled = true;
                this.UsernameTBox.Enabled = true;
                this.PasswordTBox.Enabled = true;
                this.ModUsernameTBox.Enabled = true;
                this.ModPasswordTBox.Enabled = true;
            }

            public void Authorized()
            {
                this.LoginBtn.Visibility = ViewStates.Gone;

                this.PasswordTBox.Visibility = ViewStates.Gone;
                this.LabelPassword.Visibility = ViewStates.Gone;
                this.UsernameTBox.Visibility = ViewStates.Gone;
                this.LabelUsername.Visibility = ViewStates.Gone;

                this.ModPasswordTBox.Visibility = ViewStates.Gone;
                this.LabelModPassword.Visibility = ViewStates.Gone;
                this.ModUsernameTBox.Visibility = ViewStates.Gone;
                this.LabelModUsername.Visibility = ViewStates.Gone;

                this.LabelGroupName.Visibility = ViewStates.Visible;
                this.GroupNameTBox.Visibility = ViewStates.Visible;
                this.StartBotBtn.Visibility = ViewStates.Visible;
                this.LogoutBtn.Visibility = ViewStates.Visible;
            }

            public void InProgress()
            {
                this.LabelGroupName.Enabled = false;
                this.GroupNameTBox.Enabled = false;
                this.LogoutBtn.Enabled = false;
                this.StartBotBtn.Text = @"STOP";
                SharedPreferences.PutString("GROUPNAME", this.GroupNameTBox.Text, "GROUPNAME");
            }

            public bool IsIdle() => this.StartBotBtn.Text == @"START THE BOT";

            public void Finished()
            {
                this.StartBotBtn.Text = @"START THE BOT";
                this.LogoutBtn.Enabled = true;
                this.LabelGroupName.Enabled = true;
                this.GroupNameTBox.Enabled = true;
            }
        }
    }
}