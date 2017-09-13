using System.Collections.Generic;
using System.Linq;
using Shunxi.Business.Tables;
using Shunxi.DataAccess;

namespace Shunxi.Business.Logic
{
    public static class ExceptionService
    {
        public static IList<AppException> GetUnhandleException()
        {
            using (var db = new IotContext())
            {
                return db.AppExceptions.ToList();
            }
        }

        public static void Save(AppException exception)
        {
            using (var db = new IotContext())
            {
                db.AppExceptions.Add(exception);
                db.SaveChanges();
            }
        }

        public static void MarkException(AppException exception)
        {
            using (var db = new IotContext())
            {
                db.AppExceptions.Remove(exception);
                db.SaveChanges();
            }
        }
    }
}
