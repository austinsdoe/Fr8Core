﻿using System;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseObject : IBaseDO, ICreateHook, ISaveHook, IModifyHook
    {
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public virtual void BeforeCreate()
        {
            if (CreateDate == default(DateTimeOffset))
                CreateDate = DateTimeOffset.UtcNow;
        }

        public virtual void AfterCreate()
        {
        }

        public virtual void BeforeSave()
        {
			LastUpdated = DateTimeOffset.UtcNow;
        }

        public virtual void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            this.DetectStateUpdates(originalValues, currentValues);
        }

    }
}
