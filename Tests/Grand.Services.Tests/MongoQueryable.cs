using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests
{
    public class MongoQueryable<T> : IOrderedMongoQueryable<T>
    {
		public List<T> MockData { get; set; }

		public Type ElementType => MockData.AsQueryable().ElementType;

		public Expression Expression => MockData.AsQueryable().Expression;

		public IQueryProvider Provider => MockData.AsQueryable().Provider;

		public IEnumerator<T> GetEnumerator() => MockData.AsQueryable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => MockData.AsQueryable().GetEnumerator();

		public QueryableExecutionModel GetExecutionModel() => throw new NotImplementedException();

		public IAsyncCursor<T> ToCursor(CancellationToken cancellationToken = default) => throw new NotImplementedException();

		public Task<IAsyncCursor<T>> ToCursorAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
	}
}
