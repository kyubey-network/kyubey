using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Interface
{
    public interface IBaseRepository<T>
    {
        //int Count(Expression<Func<T, bool>> predicate);
        //T Find(Expression<Func<T, bool>> whereLambda);
        //IQueryable<T> FindList<S>(Expression<Func<T, bool>> whereLamdba, bool isAsc, Expression<Func<T, S>> orderLamdba);
        //bool Exist(Expression<Func<T, bool>> anyLambda);
    }
}
