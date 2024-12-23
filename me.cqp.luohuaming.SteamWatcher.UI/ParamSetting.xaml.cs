using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// ParamSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ParamSetting : Window
    {
        public ParamSetting()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var uri = e.Uri;
            Process.Start(uri.ToString());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool initAutoLoad = AppConfig.Instance.ConfigChangeWatcher.EnableRaisingEvents;
            try
            {
                AppConfig.Instance.ConfigChangeWatcher.EnableRaisingEvents = false;
                if (VerifyInput(out string err))
                {
                    var properties = typeof(AppConfig).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    foreach (UIElement item in Container.Children)
                    {
                        if (item is TextBox textBox)
                        {
                            var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                            if (property != null && TryParse(textBox.Text, property.PropertyType, out object value))
                            {
                                property.SetValue(null, value);
                                AppConfig.Instance.SetConfig(textBox.Name, value);
                            }
                        }
                        else if (item is StackPanel stackPanel)
                        {
                            foreach (UIElement child in stackPanel.Children)
                            {
                                if (child is CheckBox checkBox)
                                {
                                    var property = properties.FirstOrDefault(x => x.Name == checkBox.Name);
                                    property?.SetValue(null, checkBox.IsChecked);
                                    AppConfig.Instance.SetConfig(checkBox.Name, checkBox.IsChecked);
                                }
                            }
                        }
                    }
                    AppConfig.GameNameFilter.Clear();
                    foreach (var item in GameNameFilter.Items)
                    {
                        AppConfig.GameNameFilter.Add(item.ToString());
                    }
                    AppConfig.Instance.SetConfig("GameNameFilter", AppConfig.GameNameFilter);
                    MainWindow.ShowInfo("配置保存成功");
                }
                else
                {
                    MainWindow.ShowError(err);
                }
            }
            catch
            {
                MainWindow.ShowError("配置保存失败，查看日志排查问题");
            }
            finally
            {
                AppConfig.Instance.ConfigChangeWatcher.EnableRaisingEvents = initAutoLoad;
            }
        }

        private static bool TryParse(string input, Type type, out object value)
        {
            value = input;
            if (type.Name == "Int32")
            {
                if (int.TryParse(input, out int v))
                {
                    value = v;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool VerifyInput(out string err)
        {
            err = "";
            var properties = typeof(AppConfig).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (UIElement item in Container.Children)
            {
                if (item is TextBox textBox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                    if (property != null && !TryParse(textBox.Text, property.PropertyType, out _))
                    {
                        err = $"{textBox.Name} 的 {textBox.Text} 输入无法转换为有效配置";
                        return false;
                    }
                }
            }
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var properties = typeof(AppConfig).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (UIElement item in Container.Children)
            {
                if (item is TextBox textBox)
                {
                    var property = properties.FirstOrDefault(x => x.Name == textBox.Name);
                    if (property != null)
                    {
                        textBox.Text = property.GetValue(null).ToString();
                    }
                }
                else if (item is StackPanel stackPanel)
                {
                    foreach (UIElement child in stackPanel.Children)
                    {
                        if (child is CheckBox checkBox)
                        {
                            var property = properties.FirstOrDefault(x => x.Name == checkBox.Name);
                            if (property != null)
                            {
                                checkBox.IsChecked = (bool)property.GetValue(null);
                            }
                        }
                    }
                }
            }
            GameNameFilter.Items.Clear();
            foreach(var item in AppConfig.GameNameFilter)
            {
                GameNameFilter.Items.Add(item);
            }
        }

        private void GameNameFilterAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GameNameFilterAdd.Text))
            {
                bool duplicate = false;
                foreach (var item in GameNameFilter.Items)
                {
                    if (item.ToString() == GameNameFilterAdd.Text)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (duplicate)
                {
                    MainWindow.ShowError("已存在相同项");
                    return;
                }
                GameNameFilter.Items.Add(GameNameFilterAdd.Text);
            }
        }

        private void GameNameFilterRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameNameFilter.SelectedIndex < 0)
            {
                MainWindow.ShowError("请选择一项");
                return;
            }
            GameNameFilter.Items.RemoveAt(GameNameFilter.SelectedIndex);
        }
    }
}