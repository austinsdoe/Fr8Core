﻿using System;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Newtonsoft.Json;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace Hub.Managers
{
    public static class CrateManagerExtensions
    {
        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDO activity)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }

        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            if (activity == null) throw new ArgumentNullException("action");
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }

        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return crateManager.UpdateStorage(() => payload.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDO activity)
        {
           return GetStorage(crateManager, activity.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, string crateStorageRaw)
        {
            if (string.IsNullOrWhiteSpace(crateStorageRaw))
            {
                return new CrateStorage();
            }

            return crateManager.FromDto(CrateStorageFromStringConverter.Convert(crateStorageRaw));
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            return crateManager.FromDto(activity.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            return crateManager.FromDto(payload.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActivityDTO activity)
        {
            return crateManager.IsEmptyStorage(activity.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActivityDO activity)
        {
            if (string.IsNullOrWhiteSpace(activity.CrateStorage))
            {
                return true;
            }

            var proxy = JsonConvert.DeserializeObject<CrateStorageDTO>(activity.CrateStorage);
            
            if (proxy.Crates == null)
            {
                return true;
            }

            return proxy.Crates.Length == 0;
        }
        /// <summary>
        /// Lets you update activity UI control values without need to unpack and repack control crates
        /// </summary>
        public static ActivityDTO UpdateControls<TActivityUi>(this ActivityDTO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return (ActivityDTO)UpdateControls((object)activity, action);
        }
        /// <summary>
        /// Lets you update activity UI control values without need to unpack and repack control crates
        /// </summary>
        public static ActivityDO UpdateControls<TActivityUi>(this ActivityDO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return (ActivityDO)UpdateControls((object)activity, action);
        }

        private static object UpdateControls<TActivityUi>(object activity, Action<TActivityUi> action) where TActivityUi  : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var crateManager = new CrateManager();
            var activityDo = activity as ActivityDO;
            var activityDto = activity as ActivityDTO;
            using (var storage = activityDo != null ? crateManager.GetUpdatableStorage(activityDo) : crateManager.GetUpdatableStorage(activityDto))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
                action(activityUi);
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }
            return activityDo != null ? (object)activityDo : activityDto;
        }
        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        public static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ActivityDO activity) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return GetReadonlyActivityUi<TActivityUi>((object)activity);
        }
        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        public static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ActivityDTO activity) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            return GetReadonlyActivityUi<TActivityUi>((object)activity);
        }

        private static TActivityUi GetReadonlyActivityUi<TActivityUi>(object activity) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            var crateManager = new CrateManager();
            var activityDo = activity as ActivityDO;
            var activityDto = activity as ActivityDTO;
            var storage = activityDo != null ? crateManager.GetStorage(activityDo) : crateManager.GetStorage(activityDto);
            return new TActivityUi().ClonePropertiesFrom(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content) as TActivityUi;
        }
    }
}