using Grand.Services.Orders;

namespace Grand.Services.Events.Web
{
    public class PlaceOrderDetailsEvent<R, O> where R : PlaceOrderResult where O : PlaceOrderContainter
    {
        private readonly R _result;
        private readonly O _containter;

        public PlaceOrderDetailsEvent(R result, O containter)
        {
            _result = result;
            _containter = containter;
        }
        public R Result { get { return _result; } }
        public O Containter { get { return _containter; } }

    }

}
