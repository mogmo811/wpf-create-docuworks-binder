using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Binder.Operation;
using Binder.Common;
using Binder.Parameter;
using System.Diagnostics;
using System.IO;
using System.Collections.Specialized;
using Binder.MyWindow.ViewModels;

namespace Binder.MyWindow.Views
{

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        FileOperation _fileOperation = new FileOperation();
        MainWindowViewModel _viewmodel = new MainWindowViewModel();
        CollectionViewSource _view = new CollectionViewSource();

        TaskScheduler uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();
        

        public MainWindow()
        {
            InitializeComponent();
            this._viewmodel = new MainWindowViewModel();
            this.DataContext = this._viewmodel;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // コントロール初期化
            this.FilterComboBox.SelectedIndex = 0;
        }

        // リストがロードされたときの処理
        private void PathListView_Loaded(object sender, RoutedEventArgs e)
        {
            this._view.Source = this._viewmodel.GetFileInformationList();
            this.PathListView.DataContext = this._view;
        }

        #region ファイル操作        
        // リストを初期化
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this._viewmodel.InitializeList();
        }        
        #endregion

        // 
        private void PathListView_KeyDown(object sender, KeyEventArgs e)
        {
            //Deleteキーが押されたか調べる
            if (e.Key == Key.Delete) this.DeleteItem();

            //Ctrl + ↓
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Down)
                Debug.WriteLine("Controlキー＋Downが押されています。");
            
            //Ctrl + ↑
        }

        private bool isDownKey(Key key)
        {
            return (Keyboard.GetKeyStates(key) & KeyStates.Down) == KeyStates.Down;
        }

        private void DeleteItem()
        {
            if (this.PathListView.SelectedItem != null)
            {
                var items = this.PathListView.SelectedItems; // IList
                var list = new List<FileInformation>(); // List

                foreach (var item in items)
                    list.Add((FileInformation)item);

                foreach(var item in list)
                    this._viewmodel.RemoveCollection(item);
            }
        }

        #region ドラッグ&ドロップ処理

        private Point _startPoint = new Point();
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;
        private const string _selectedItemCollection = "System.Windows.Controls.SelectedItemCollection";

        // クリック時
        private void PathListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this.PathListView); // ポイント取得
        }

        // 移動時
        private void PathListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(this.PathListView);

                if (Math.Abs(point.X - this._startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(point.Y - this._startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    BeginDrag(e);
                }
            }
        }

        private void PathListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }

        // ドロップ動作
        private void PathListView_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // ファイルをドロップした時
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ファイルリスト取得
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (files == null) return;

                foreach (var path in files)
                {
                    List<string> filepaths = new List<string>();
                    // フィルタ文字列を取得
                    string searchPattern = this._viewmodel.ConvertSearchPatternString(this.FilterComboBox.SelectedValue);

                    if (Directory.Exists(path)) // フォルダの場合
                        filepaths = this._fileOperation.GetFilePathInFolder(path, searchPattern);
                    else if (File.Exists(path)) // ファイルの場合
                        filepaths.Add(path);                    

                    // パスをリストコントロールに表示
                    this._viewmodel.ConvertToFileInformationList(filepaths);
                }
            }

            // 順序入れ替えの時
            if (e.Data.GetDataPresent("myFormat"))
            {
                FileInformation item = e.Data.GetData("myFormat") as FileInformation;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    FileInformation itemToReplace = (FileInformation)PathListView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    int index = PathListView.Items.IndexOf(itemToReplace);

                    if (index >= 0)
                        this._viewmodel.InsertCollection(index, item);
                }
                else
                    this._viewmodel.AddCollection(item);
            }
        }

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(this.PathListView as Visual);
            _layer.Add(_adorner);
        }

        private void BeginDrag(MouseEventArgs e)
        {
            ListView listView = this.PathListView;
            ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItems
            FileInformation item = (FileInformation)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);


            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += PathListView_DragEnter;

            DataObject data = new DataObject("myFormat", item);
            DragDropEffects de = DragDrop.DoDragDrop(this.PathListView, data, DragDropEffects.Move);

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= PathListView_DragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        void ListViewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this._dragIsOutOfScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }

        // ゴーストイメージの座標を更新，ドラッグ中のマウス位置に合わせて表示する。
        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(this.PathListView).X;
                _adorner.OffsetTop = args.GetPosition(this.PathListView).Y - _startPoint.Y;

            }
        }

        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == this.PathListView)
            {
                Rect rect = VisualTreeHelper.GetContentBounds(this.PathListView);
                if (!rect.Contains(rect))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }


        #endregion

        // コンテキストメニューイベント
        private void RemoveNutritionContextMenu_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteItem();
        }

        // キャンセルボタン
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this._viewmodel.CancelOn();
        }
        
        // エクスプローラで開く
        private void OpenSaveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.SaveFolderText.Text);
        }

        // 除外リストを編集
        private void NotTargetEditButton_Click(object sender, RoutedEventArgs e)
        {
            this._viewmodel.EditNotTarget();
        }

        // ヘッダクリックによる操作
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader columnHeader = sender as GridViewColumnHeader;
            string columnTagName = columnHeader.Tag as string;

            Binding binding = (Binding)columnHeader.Column.DisplayMemberBinding;
            string path = binding.Path.Path;

            var preSortItems = this._viewmodel.GetFileInformationList(); // this.PathListView.ItemsSource.Cast<object>();
            //並べ替え
            var sortedItems = new ObservableCollection<FileInformation>(preSortItems.OrderBy(n => n.GetType().GetProperty(path).GetValue(n)));
            

            // すでに昇順なら降順にする
            if (sortedItems.SequenceEqual(preSortItems))
            {
                sortedItems = new ObservableCollection<FileInformation>(sortedItems.Reverse());
            }
            
            this._viewmodel.ChangeFilesInformationList(sortedItems);
                       
        }

        // ログが更新されたら
        private void LogListBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            (this.LogListBox.ItemsSource as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(listBox_CollectionChanged);
        }

        void listBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.LogListBox.ScrollIntoView(this.LogListBox.Items[this.LogListBox.Items.Count - 1]);
        }
        
    }
}