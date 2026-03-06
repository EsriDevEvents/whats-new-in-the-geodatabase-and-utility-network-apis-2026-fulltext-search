using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FullTextSearchDevSummitDemo
{
  public class DatasetField
  {
    public bool IsSelected { get; set; }

    private string _fieldName = "";
    public string FieldName
    {
      get { return _fieldName; }
      set { _fieldName = value; }
    }
  }

  public class SearchDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "FullTextSearchDevSummitDemo_SearchDockPane";
    private ObservableCollection<string> _datasets = new ObservableCollection<string>();
    private ObservableCollection<DatasetField> _datasetFields = new ObservableCollection<DatasetField>();
    private IEnumerable<MapMember> _mapMembers = null;



    protected SearchDockPaneViewModel()
    {
      _mapMembers = MapView.Active.Map.GetMapMembersAsFlattenedList();
      ReadDatasets();

    }
    private void ReadDatasets()
    {
      IEnumerable<string> layers = _mapMembers.Select(l => l.Name);
      _datasets = new ObservableCollection<string>(layers);
    }

    private ICommand _selectionCommand = null;
    public ICommand SelectionCommand
    {
      get
      {
        if (_selectionCommand == null)
        {
          _selectionCommand = new RelayCommand(ExecuteSearch);
        }
        return _selectionCommand;
      }
    }


    private ICommand _searchCommand = null;
    public ICommand SearchCommand
    {
      get
      {
        if (_searchCommand == null)
        {
          _searchCommand = new RelayCommand(ExecuteSearch);
        }
        return _searchCommand;
      }
    }
    private bool? _selectedRadio = null;
    public bool? SelectedRadio
    {
      get { return _selectedRadio; }
      set
      {
        if (_selectedRadio != value)
        {
          _selectedRadio = value;
          SetProperty(ref _selectedRadio, value, () => SelectedRadio);
        }
      }
    }

    public ObservableCollection<string> Datasets
    {
      get
      {
        return _datasets;
      }
      set
      {
        SetProperty(ref _datasets, value, () => Datasets);
      }
    }

    public ObservableCollection<DatasetField> DatasetFields
    {
      get
      {
        return _datasetFields;
      }
      set
      {
        SetProperty(ref _datasetFields, value, () => DatasetFields);
      }
    }


    private string _selectedDataset;
    public string SelectedDataset
    {
      get => _selectedDataset;
      set
      {
        SetProperty(ref _selectedDataset, value, () => SelectedDataset);
        HandleDatasetSelection(_selectedDataset);
      }
    }

    private string _selectedField;
    public string SelectedField
    {
      get => _selectedField;
      set
      {
        SetProperty(ref _selectedField, value, () => SelectedField);
      }
    }

    private string _searchTerm = null;
    public string SearchTerm
    {
      get => _searchTerm;
      set
      {
        SetProperty(ref _searchTerm, value, () => SearchTerm);
      }
    }

    private string _searchExpression = null;
    public string SearchExpression
    {
      get => _searchExpression;
      set
      {
        SetProperty(ref _searchExpression, value, () => SearchExpression);
      }
    }

    private string _searchResult = null;
    public string SearchResult
    {
      get => _searchResult;
      set
      {
        SetProperty(ref _searchResult, value, () => SearchResult);
      }
    }

    private void HandleDatasetSelection(string selectedDataset)
    {
      QueuedTask.Run(() =>
      {
        IEnumerable<DatasetField> datasetFields = new List<DatasetField>();
        IEnumerable<string> fieldNames = new List<string>();

        foreach (var item in _mapMembers)
        {
          if (item.Name == selectedDataset)
          {
            if (item.GetType() == typeof(FeatureLayer))
            {
              FeatureLayer featureLayer = _mapMembers.First(m => m.GetType() == typeof(FeatureLayer)) as FeatureLayer;
              fieldNames = featureLayer.GetFeatureClass().GetDefinition().GetFields().Select(f => f.Name);

              foreach (var fieldName in fieldNames)
              {
                datasetFields = datasetFields.Append(new DatasetField() { FieldName = fieldName, IsSelected = false });
              }
            }
            else if (item.GetType() == typeof(StandaloneTable))
            {
              StandaloneTable standaloneTable = _mapMembers.First(m => m.GetType() == typeof(StandaloneTable)) as StandaloneTable;
              fieldNames = standaloneTable.GetTable().GetDefinition().GetFields().Select(f => f.Name);
              foreach (var fieldName in fieldNames)
              {
                datasetFields = datasetFields.Append(new DatasetField() { FieldName = fieldName, IsSelected = false });
              }
            }
          }
        }
        DatasetFields = new ObservableCollection<DatasetField>(datasetFields);
      });
    }

    private void ExecuteSearch()
    {
      SearchResult = string.Empty;

      var searchTerm = SearchTerm;
      var searchExpression = SearchExpression;
      var pp = DatasetFields;
      var radio = SelectedRadio;
      
      SearchResult = DatasetFields.Select(x=>x.FieldName).ToString() + searchTerm + searchExpression + radio.ToString();
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  public class SearchDockPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      SearchDockPaneViewModel.Show();
    }
  }

}
