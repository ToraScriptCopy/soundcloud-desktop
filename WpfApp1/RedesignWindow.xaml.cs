using System.Windows;
using UiControls = Wpf.Ui.Controls;
namespace WpfApp1
{
    public partial class RedesignWindow : UiControls.FluentWindow
    {
        private readonly MainWindow _owner;
        private readonly AppState _state;
        private bool _initialized;
        public RedesignWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            _state = owner.State;
            SwCards.IsChecked = _state.RdCards;
            SwButtons.IsChecked = _state.RdButtons;
            SwHeader.IsChecked = _state.RdHeader;
            SwPlayer.IsChecked = _state.RdPlayer;
            SwComments.IsChecked = _state.RdComments;
            SwSidebar.IsChecked = _state.RdSidebar;
            SwInputs.IsChecked = _state.RdInputs;
            SwPopups.IsChecked = _state.RdPopups;
            ApplyLoc();
            _initialized = true;
            Loaded += delegate { Fx.Enter(this); };
        }
        public void ApplyLoc()
        {
            Title = Loc.Get("RdTitle");
            RdBar.Title = Loc.Get("RdTitle");
            SwCards.Content = Loc.Get("RdCards");
            SwButtons.Content = Loc.Get("RdButtons");
            SwHeader.Content = Loc.Get("RdHeader");
            SwPlayer.Content = Loc.Get("RdPlayer");
            SwComments.Content = Loc.Get("RdComments");
            SwSidebar.Content = Loc.Get("RdSidebar");
            SwInputs.Content = Loc.Get("RdInputs");
            SwPopups.Content = Loc.Get("RdPopups");
        }
        private void Sw_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.RdCards = SwCards.IsChecked == true;
            _state.RdButtons = SwButtons.IsChecked == true;
            _state.RdHeader = SwHeader.IsChecked == true;
            _state.RdPlayer = SwPlayer.IsChecked == true;
            _state.RdComments = SwComments.IsChecked == true;
            _state.RdSidebar = SwSidebar.IsChecked == true;
            _state.RdInputs = SwInputs.IsChecked == true;
            _state.RdPopups = SwPopups.IsChecked == true;
            _owner.SaveAllState();
            _owner.ReapplySiteCss();
        }
    }
}
