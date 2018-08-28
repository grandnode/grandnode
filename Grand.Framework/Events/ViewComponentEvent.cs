using Grand.Framework.Components;

namespace Grand.Framework.Events
{
    public class ViewComponentEvent<T, U> where U : BaseViewComponent
    {
        private readonly T _model;
        private readonly U _vieComponent;
        private readonly string _viewName;

        public ViewComponentEvent(string viewname, U vieComponent)
        {
            _viewName = viewname;
            _vieComponent = vieComponent;
        }

        public ViewComponentEvent(string viewname, T model, U vieComponent)
        {
            _viewName = viewname;
            _model = model;
            _vieComponent = vieComponent;
        }

        public ViewComponentEvent(T model, U vieComponent)
        {
            _model = model;
            _vieComponent = vieComponent;
        }

        public T Model { get { return _model; } }
        public U VComponent { get { return _vieComponent; } }
        public string ViewName { get { return _viewName; } }
    }
}
