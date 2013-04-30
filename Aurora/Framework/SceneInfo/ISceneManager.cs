﻿using System.Collections.Generic;
using Nini.Config;

namespace Aurora.Framework.SceneInfo
{

    #region Delegates

    public delegate void NewScene(IScene scene);

    public delegate void NoParam();

    #endregion

    public interface ISceneManager
    {
        /// <summary>
        ///     Starts a region
        /// </summary>
        /// <param name="newRegion"></param>
        void StartRegion(out bool newRegion);

        /// <summary>
        ///     Shuts down the given region
        /// </summary>
        /// <param name="shutdownType"></param>
        /// <param name="p"></param>
        void CloseRegion(ShutdownType shutdownType, int p);

        /// <summary>
        ///     Removes and resets terrain and objects from the database
        /// </summary>
        void ResetRegion();

        /// <summary>
        ///     Restart the given region
        /// </summary>
        void RestartRegion();

        void HandleStartupComplete(List<string> data);

        ISimulationDataStore GetSimulationDataStore();

        IConfigSource ConfigSource { get; }

        IScene Scene { get; }

        event NewScene OnCloseScene;
        event NewScene OnAddedScene;
        event NewScene OnFinishedAddingScene;
    }
}