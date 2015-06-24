using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class ChildContentViewFactory
    {
        [ImportingConstructor]
        public ChildContentViewFactory()
        {
        }

        [ImportMany(typeof (IChildContentViewModel))]
        private IEnumerable<Lazy<IChildContentViewModel, IChildContentViewModelMetaData>> ChildViewModelList { get; set;
        }

        public IChildContentViewModel GetChildContentViewModel(ChildViewContentType childViewContentType)
        {
            if (!ChildViewModelList.Any())
            {
                return null;
            }

            var viewModelInstance =
                ChildViewModelList
                    .FirstOrDefault(list => list.Metadata.ChildViewContentType == childViewContentType);
            if (viewModelInstance != null)
            {
                return viewModelInstance.Value;
            }
            return null;
        }
    }
}