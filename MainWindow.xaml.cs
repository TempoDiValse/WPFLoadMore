using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections;
using mshtml;
using Tester;
using WebConnect;

namespace WpfApplication1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int LOAD_COUNT = 30;

        private WebConnection conn;
        private DispatcherTimer timer;
        private int t_count = 1;
        private int loadCount = LOAD_COUNT;

        private ScrollViewer scroller;

        public MainWindow() {
            InitializeComponent();

            conn = new WebConnection("[URL]");
            conn.setCallback(result);
            Dictionary<string, string> querys = new Dictionary<string, string>();

            querys.Add("page", "1");
            querys.Add("loadCount", ""+loadCount);
            conn.setParameter(querys);
            querys = null;

            // 파라미터까지 장전 하면 프로그램이 실행되는 최초 시점에 onLoad를 호출하면서 데이터를 가져오는 페이지에 접근한다.
            // 파라미터를 넘겨주는 부분에서 POST 방식은 전혀 고려하지 않고 GET 방식으로 만들었다. ( 실습용 )
            
            Loaded += onLoad;
            
            itemExit.Click += exit;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tick;

            count.Text = t_count + "s";

            // 타이머는 1초에 한번씩 카운터가 올라가고 10이 채워지면 리스트를 한번 새로고침하여 데이터를 최신화 한다.
        }
        
        public void onScroll(object sender, ScrollChangedEventArgs e) {
            /*
                스크롤 오프셋은 2가지 타입이 있는데, 하나는 컨텐츠 인덱스 값과 다른 하나는 스크롤 Y좌표 값이다.
                이것은 Xaml에서 정의한 ListView 속성에 ScrollViewer.CanContentScroll의 T/F에 따라 설정할 수 있다.
                지금은 컨텐츠의 갯수에 따라 하도록 만들었다.

                컨텐츠가 최대로 보여질 수 있는 갯수와 현재 보여지는 갯수가 같으면 다음 데이터를 로드 하는데, 
                다음 데이터를 로드하고 나서 스크롤의 위치를 조정하지 않으면 계속 끝에 머물러 있어서 계속 다음 데이터를 불러온다.
                그래서 로드하기 이전 위치에 머무를 수 있도록 했다.
            */
            var maxOffset = scroller.ScrollableHeight;
            var currentOffset = e.VerticalOffset;

            if (maxOffset == currentOffset) {
                Dictionary<string, string> querys = new Dictionary<string, string>();

                querys.Add("page", "1");
                querys.Add("loadCount", ""+(loadCount+=LOAD_COUNT));
                conn.setParameter(querys);
                querys = null;

                t_count = 0;

                conn.load();
                scroller.ScrollToVerticalOffset(loadCount - LOAD_COUNT);
            }
        }

        public void onLoad(object sender, RoutedEventArgs e) {
            conn.load();
        }

        public void tick(object sender, EventArgs e) {
            if(t_count != 10) {
                t_count++;
            } else {
                t_count = 1;
                conn.load();
            }

            count.Text = t_count + "s";
        }

        public void result(string response) {
            JObject root = JObject.Parse(response); // Newton JSON Library

            /*
                ... Arrange list items... 
            */
            
            if (listView.HasItems) {
                listView.SelectionChanged -= itemOnClick;

                //리스트 아이템을 클릭하면 액션이 일어날 수 있도록 딜리게이트 정보를 넘겨주지만, 
                //데이터를 불러올 때 최초 등록하면 되는게 아니라 중복으로 들어가기 때문에
                //이벤트 딜리게이트를 지정하기 전에 이전에 들어갔던 딜리게이트를 제거하고 한다.
            }

            listView.ItemsSource = items;
            listView.SelectionChanged += itemOnClick;
            listView.Items.Refresh();

            if (scroller == null) {
                Border border = (Border)VisualTreeHelper.GetChild(flashList, 0);
                scroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                
                scroller.ScrollChanged += onScroll;
                // ListView에서는 직접 스크롤을 감지할 수 있는 리스너에 접근 할 수 없기 때문에, 
                // UI Tree를 이용하여 Child로 있는 ScrollViewer에 접근해야 한다.
                // ScrollViewer는 ScrollChanged라는 딜리게이트로 스크롤 위치를 알 수 있기 때문에
                // 최초에 불러올 때 여기에 onScroll을 등록하여 스크롤 정보를 받아올 수 있도록 한다.
            }

            timer.Start();
        }

        public void itemOnClick(object sender, RoutedEventArgs e) {
            var obj = (ListObj)(sender as ListView).SelectedItem;
            
        }
        
        public void exit(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show("종료하시겠습니까?", "종료", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if(result == MessageBoxResult.Yes) {
                App.Current.Shutdown();
            }
        }
    }
}
