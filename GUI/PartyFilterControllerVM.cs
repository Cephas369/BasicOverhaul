using System;
using System.Collections.Generic;
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
    private readonly PartyScreenLogic.PartyRosterSide _rosterSide;
    private readonly Action<PartyScreenLogic.PartyRosterSide, (FilterType filterType, object selected)> _onChange;
    private (FilterType filterType, object selected) _selectedItem;
    private bool _isAscending;
    private bool _isCustomSort;
    private int _marginLeft;
    public FilterType FilterType;
    private SelectorVM<TroopFilterSelectorItemVM> _sortOptions;

    public PartyFilterControllerVM(PartyScreenLogic.PartyRosterSide rosterSide, Action<PartyScreenLogic.PartyRosterSide, (FilterType filterType, object selected)> onChange, List<TroopFilterSelectorItemVM> elements, int marginLeft, FilterType filterType)
    {
      FilterType = filterType;
      _rosterSide = rosterSide;
      SortOptions = new SelectorVM<TroopFilterSelectorItemVM>(-1, OnSortSelected);

      foreach (var element in elements)
      {
        SortOptions.AddItem(element);
      }

      SortOptions.SelectedIndex = SortOptions.ItemList.Count - 1;
      _onChange = onChange;
      MarginLeft = marginLeft;
    }

    private void OnSortSelected(SelectorVM<TroopFilterSelectorItemVM> selector)
    {
      _selectedItem = (selector.SelectedItem.FilterType, selector.SelectedItem.Item);
      Action<PartyScreenLogic.PartyRosterSide, (FilterType filterType, object selected)> onSort = _onChange;
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
      Action<PartyScreenLogic.PartyRosterSide, (FilterType filterType, object selected)> onSort = _onChange;
      if (onSort == null)
        return;
      onSort(_rosterSide, (FilterType, selectedItem));
    }

    public void ExecuteToggleOrder()
    {
      Action<PartyScreenLogic.PartyRosterSide, (FilterType filterType, object selected)> onSort = _onChange;
      if (onSort == null)
        return;
      onSort(_rosterSide, _selectedItem);
    }

    [DataSourceProperty]
    public bool IsCustomSort
    {
      get => _isCustomSort;
      set
      {
        if (value == _isCustomSort)
          return;
        _isCustomSort = value;
        OnPropertyChangedWithValue(value);
      }
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
  }
}
