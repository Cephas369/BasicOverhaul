// Decompiled with JetBrains decompiler
// Type: TaleWorlds.CampaignSystem.ViewModelCollection.Party.TroopSortSelectorItemVM
// Assembly: TaleWorlds.CampaignSystem.ViewModelCollection, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C2B4842E-B3B8-46FE-A96B-C6CECFB63981
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.CampaignSystem.ViewModelCollection.dll

using BasicOverhaul.Patches;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Localization;

namespace BasicOverhaul
{
    public class TroopFilterSelectorItemVM : SelectorItemVM
    {
        public object Item { get; private set; }
        public FilterType FilterType { get; private set; }

        public TroopFilterSelectorItemVM(TextObject s, FilterType filterType, object item): base(s)
        {
            Item = item;
            FilterType = filterType;
        }
    }
}