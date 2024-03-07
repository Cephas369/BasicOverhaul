using System;
using System.Collections.Generic;
using BasicOverhaul.GUI;
using BasicOverhaul.Patches;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BasicOverhaul
{
  public class PartyFilterControllerVM : ViewModel
  {
    private readonly Side _rosterSide;
    private readonly Action<Side, (FilterType filterType, object selected)> _onChange;
    private (FilterType filterType, object selected) _selectedItem;
    private bool _isCustomSort;
    private int _marginLeft;
    private int _marginRight;
    private int _marginTop;
    private bool _isVisible;
    private readonly FilterType _filterType;
    private SelectorVM<TroopFilterSelectorItemVM> _sortOptions;

    public PartyFilterControllerVM(Side rosterSide, Action<Side, (FilterType filterType, object selected)> onChange, List<TroopFilterSelectorItemVM> elements,
      int marginLeft, int marginRight, int marginTop, FilterType filterType)
    {
      _filterType = filterType;
      _rosterSide = rosterSide;
      SortOptions = new SelectorVM<TroopFilterSelectorItemVM>(-1, OnSortSelected);

      foreach (var element in elements)
      {
        SortOptions.AddItem(element);
      }

      SortOptions.SelectedIndex = 0;
      _onChange = onChange;
      MarginLeft = marginLeft;
      MarginTop = marginTop;
      MarginRight = marginRight;
      IsVisible = true;
    }

    private void OnSortSelected(SelectorVM<TroopFilterSelectorItemVM> selector)
    {
      _selectedItem = (selector.SelectedItem.FilterType, selector.SelectedItem.Item);
      Action<Side, (FilterType filterType, object selected)> onSort = _onChange;
      if (onSort == null)
        return;
      onSort(_rosterSide, _selectedItem);
    }

    public void SelectSortType(object selectedItem)
    {
      for (int index = 0; index < SortOptions.ItemList.Count; ++index)
      {
        if (SortOptions.ItemList[index].Item == selectedItem)
          SortOptions.SelectedIndex = index;
      }
    }

    public void SortWith(object selectedItem, bool isAscending)
    {
      Action<Side, (FilterType filterType, object selected)> onSort = _onChange;
      if (onSort == null)
        return;
      onSort(_rosterSide, (_filterType, selectedItem));
    }

    public void ExecuteToggleOrder()
    {
      Action<Side, (FilterType filterType, object selected)> onSort = _onChange;
      if (onSort == null)
        return;
      onSort(_rosterSide, _selectedItem);
    }

    [DataSourceProperty]
    public SelectorVM<TroopFilterSelectorItemVM> SortOptions
    {
      get => _sortOptions;
      set
      {
        if (value == _sortOptions)
          return;
        _sortOptions = value;
        OnPropertyChangedWithValue(value);
      }
    }
    [DataSourceProperty]
    public bool IsVisible
    {
      get => _isVisible;
      set
      {
        if (value == _isVisible)
          return;
        _isVisible = value;
        OnPropertyChangedWithValue(value);
      }
    }
    [DataSourceProperty]
    public int MarginLeft
    {
      get => _marginLeft;
      set
      {
        if (value == _marginLeft)
          return;
        _marginLeft = value;
        
        OnPropertyChangedWithValue(value);
      }
    }
    [DataSourceProperty]
    public int MarginRight
    {
      get => _marginRight;
      set
      {
        if (value == _marginRight)
          return;
        _marginRight = value;
        
        OnPropertyChangedWithValue(value);
      }
    }
    
    [DataSourceProperty]
    public int MarginTop
    {
      get => _marginTop;
      set
      {
        if (value == _marginTop)
          return;
        _marginTop = value;
        
        OnPropertyChangedWithValue(value);
      }
    }
  }
}
