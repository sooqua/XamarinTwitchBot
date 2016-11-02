// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Inviter fragment.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Fragments
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Android.App;
    using Android.OS;
    using Android.Views;
    using Android.Widget;

    using Common;
    using Common.Exceptions;

    using Newtonsoft.Json;

    using XamarinTwitchBot.Inviter;

    public class InviterFragment : Fragment
    {
        private InviterBot inviterBot;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // base.OnCreateView(inflater, container, savedInstanceState);
            var linearLayout = (LinearLayout)inflater.Inflate(Resource.Layout.Inviter, container, false);

            var ui = new UI(linearLayout);

            this.inviterBot = new InviterBot(ui);

            ui.LoginBtn.Click += async delegate
            {
                try
                {
                    ui.PendingAuthorization();
                    await this.inviterBot.LogInAsync(ui.UsernameTBox.Text, ui.PasswordTBox.Text);
                    if (!string.IsNullOrWhiteSpace(await this.inviterBot.GetOAuthAsync()))
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
            
            ui.InviteBtn.Click += async delegate
            {
                Utils.LogDebug("Someone clicked the InviteBtn");
                if (ui.IsIdle())
                {
                    Utils.LogDebug("UI is idle. ui.StartBotBtn.Text = " + ui.InviteBtn.Text);
                    ui.InProgress();

                    while (true)
                    {
                        Utils.LogDebug("New iteration before StartInvitingAsync has occurred");
                        try
                        {
                            await this.inviterBot.StartInvitingAsync(ui.GroupTBox.Text, ui.ChannelTBox.Text, this.cts.Token);
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
                                Utils.LogError("Operation canceled unexpectedly. InviterBot is going to be re-created");
                                this.inviterBot = new InviterBot(ui);
                            }

                            break;
                        }
                        catch (OAuthIsNullException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (NotAllowedToInviteException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (IrcChannelNotFoundException ex)
                        {
                            Utils.LogError(ex.ToString());
                        }
                        catch (NoChatterFoundException ex)
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
                        catch (Exception ex)
                        {
                            Utils.LogError("SOME WEIRD SHIT JUST HAPPENED, ANALYZE YOUR DAMN PROGRAM");
                            Utils.LogWtf(ex.ToString());
                        }
                        finally
                        {
                            this.inviterBot.Stats.Clear();
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
                                                        this.inviterBot.DeleteCookies();
                                                        this.inviterBot = new InviterBot(ui);
                                                        ui.UnAuthorized();
                                                    });
                confirmDialog.SetNegativeButton("No", delegate { });
                confirmDialog.Show();
            };

            if (this.inviterBot.IsAuthenticated())
            {
                ui.PendingAuthorization();
                Task.Run(async () =>
                {
                    try
                    {
                        await this.inviterBot.GetOAuthAsync();
                        if (!string.IsNullOrWhiteSpace(this.inviterBot.OAuth))
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
        
        // ReSharper disable once InconsistentNaming
        public class UI
        {
            public readonly TextView LabelUsername;
            public readonly EditText UsernameTBox;
            public readonly TextView LabelPassword;
            public readonly EditText PasswordTBox;
            public readonly Button LoginBtn;
            public readonly TextView LabelTargetChannel;
            public readonly EditText ChannelTBox;
            public readonly TextView LabelGroupName;
            public readonly EditText GroupTBox;
            public readonly TextView LabelTasks;
            public readonly EditText TasksNBox;
            public readonly Button InviteBtn;
            public readonly TextView LabelInvited;
            public readonly TextView LabelBlacklisted;
            public readonly TextView LabelTotal;
            public readonly Button LogoutBtn;

            public UI(View view)
            {
                this.LabelUsername = view.FindViewById<TextView>(Resource.Id.labelUsername);
                this.UsernameTBox = view.FindViewById<EditText>(Resource.Id.usernameTBox);
                this.LabelPassword = view.FindViewById<TextView>(Resource.Id.labelPassword);
                this.PasswordTBox = view.FindViewById<EditText>(Resource.Id.passwordTBox);
                this.LoginBtn = view.FindViewById<Button>(Resource.Id.loginBtn);

                this.LabelTargetChannel = view.FindViewById<TextView>(Resource.Id.labelTargetChannel);
                this.ChannelTBox = view.FindViewById<EditText>(Resource.Id.channelTBox);
                this.LabelGroupName = view.FindViewById<TextView>(Resource.Id.labelGroupName);
                this.GroupTBox = view.FindViewById<EditText>(Resource.Id.groupTBox);

                this.LabelTasks = view.FindViewById<TextView>(Resource.Id.labelTasks);
                this.TasksNBox = view.FindViewById<EditText>(Resource.Id.tasksNBox);

                this.InviteBtn = view.FindViewById<Button>(Resource.Id.inviteBtn);

                this.LabelInvited = view.FindViewById<TextView>(Resource.Id.labelInvited);
                this.LabelBlacklisted = view.FindViewById<TextView>(Resource.Id.labelBlacklisted);
                this.LabelTotal = view.FindViewById<TextView>(Resource.Id.labelTotal);

                this.LogoutBtn = view.FindViewById<Button>(Resource.Id.logoutBtn);

                this.UsernameTBox.Text = SharedPreferences.GetString("USERNAME", "USERNAME");
                this.PasswordTBox.Text = SharedPreferences.GetString("PASSWORD", "PASSWORD");
                this.GroupTBox.Text = SharedPreferences.GetString("GROUP", "GROUP");
            }

            public void UnAuthorized()
            {
                this.LogoutBtn.Visibility = ViewStates.Gone;
                this.LabelTotal.Visibility = ViewStates.Gone;
                this.LabelBlacklisted.Visibility = ViewStates.Gone;
                this.LabelInvited.Visibility = ViewStates.Gone;
                this.InviteBtn.Visibility = ViewStates.Gone;
                this.TasksNBox.Visibility = ViewStates.Gone;
                this.LabelTasks.Visibility = ViewStates.Gone;
                this.GroupTBox.Visibility = ViewStates.Gone;
                this.LabelGroupName.Visibility = ViewStates.Gone;
                this.ChannelTBox.Visibility = ViewStates.Gone;
                this.LabelTargetChannel.Visibility = ViewStates.Gone;
                this.LabelUsername.Visibility = ViewStates.Visible;
                this.UsernameTBox.Visibility = ViewStates.Visible;
                this.LabelPassword.Visibility = ViewStates.Visible;
                this.PasswordTBox.Visibility = ViewStates.Visible;
                this.LoginBtn.Visibility = ViewStates.Visible;
                this.UsernameTBox.Enabled = true;
                this.PasswordTBox.Enabled = true;
                this.LoginBtn.Enabled = true;
            }

            public void PendingAuthorization()
            {
                SharedPreferences.PutString("USERNAME", this.UsernameTBox.Text, "USERNAME");
                SharedPreferences.PutString("PASSWORD", this.PasswordTBox.Text, "PASSWORD");
                this.LoginBtn.Enabled = false;
                this.UsernameTBox.Enabled = false;
                this.PasswordTBox.Enabled = false;
            }

            public void FailedAuthorization()
            {
                this.LoginBtn.Enabled = true;
                this.UsernameTBox.Enabled = true;
                this.PasswordTBox.Enabled = true;
            }

            public void Authorized()
            {
                this.LoginBtn.Visibility = ViewStates.Gone;
                this.PasswordTBox.Visibility = ViewStates.Gone;
                this.LabelPassword.Visibility = ViewStates.Gone;
                this.UsernameTBox.Visibility = ViewStates.Gone;
                this.LabelUsername.Visibility = ViewStates.Gone;
                this.LabelTargetChannel.Visibility = ViewStates.Visible;
                this.ChannelTBox.Visibility = ViewStates.Visible;
                this.LabelGroupName.Visibility = ViewStates.Visible;
                this.GroupTBox.Visibility = ViewStates.Visible;
                this.LabelTasks.Visibility = ViewStates.Visible;
                this.TasksNBox.Visibility = ViewStates.Visible;
                this.InviteBtn.Visibility = ViewStates.Visible;
                this.LabelInvited.Visibility = ViewStates.Visible;
                this.LabelBlacklisted.Visibility = ViewStates.Visible;
                this.LabelTotal.Visibility = ViewStates.Visible;
                this.LogoutBtn.Visibility = ViewStates.Visible;
            }

            public void InProgress()
            {
                this.LogoutBtn.Enabled = false;
                this.ChannelTBox.Enabled = false;
                this.GroupTBox.Enabled = false;
                this.TasksNBox.Enabled = false;
                this.InviteBtn.Text = @"STOP";
                SharedPreferences.PutString("GROUP", this.GroupTBox.Text, "GROUP");
            }

            public bool IsIdle() => this.InviteBtn.Text == @"START INVITING";

            public void Finished()
            {
                this.InviteBtn.Text = @"START INVITING";
                this.LogoutBtn.Enabled = true;
                this.ChannelTBox.Enabled = true;
                this.GroupTBox.Enabled = true;
                this.TasksNBox.Enabled = true;
            }
        }
    }
}