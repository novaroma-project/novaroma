using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using Novaroma.Interface;
using Novaroma.Interface.Model;
using Novaroma.Model;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.ViewModels;
using Novaroma.Win.Views;
using Quartz;
using Application = System.Windows.Application;

namespace Novaroma.Win {

    internal static class Helper {

        internal static string ParseArg(string argStr, string argKey) {
            argStr = argStr.Trim();
            return argStr.StartsWith(argKey, StringComparison.OrdinalIgnoreCase)
                ? argStr.Substring(argKey.Length)
                : string.Empty;
        }

        internal static void ShowMainWindow() {
            var mainWindow = IoCContainer.Resolve<MainWindow>();
            Application.Current.Dispatcher.Invoke(() => {
                Application.Current.MainWindow = mainWindow;
                mainWindow.ForceShow();
            });
        }

        internal static void EditSettings(INovaromaEngine engine, IDialogService dialogService, IConfigurable configurable, Window ownerWindow = null) {
            var settingsViewModel = new SettingsViewModel(engine, dialogService, configurable);
            var settingsWindow = new SettingsWindow(settingsViewModel);
            settingsWindow.Owner = ownerWindow;
            settingsWindow.ShowDialog();
        }

        internal static void WatchDirectory(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string directory = null) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            viewModel.WatchDirectory(directory);

            new NewMediaWizard(viewModel).ForceShow();
        }

        internal static Task AddFromDirectories(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string[] directories) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            var t = viewModel.AddFromDirectories(directories);

            new NewMediaWizard(viewModel).ForceShow();
            return t;
        }

        internal static Task AddFromSearch(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string searchQuery = null, string directory = null, bool isParentDirectory = false) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            var t = viewModel.AddFromSearch(searchQuery);

            new NewMediaWizard(viewModel).ForceShow();
            return t;
        }

        internal static Task AddFromImdbId(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string imdbId) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            var t = viewModel.AddFromImdbId(imdbId);

            new NewMediaWizard(viewModel).ForceShow();
            return t;
        }

        internal static Task NewMedia(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string searchQuery = null, string parentDirectory = null) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            var t = viewModel.AddFromSearch(searchQuery, parentDirectory);

            new NewMediaWizard(viewModel).ForceShow();
            return t;
        }

        public static void DiscoverMedia(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string parentDirectory = null) {
            var viewModel = new NewMediaWizardViewModel(engine, exceptionHandler, dialogService);
            viewModel.AddFromDiscover(parentDirectory);

            new NewMediaWizard(viewModel).ForceShow();
        }

        public static Task ManualDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, Movie movie) {
            var downloadable = (IDownloadable)movie;
            return ManualDownload(engine, exceptionHandler, dialogService, movie, downloadable.GetSearchQuery(), downloadable.VideoQuality, downloadable.ExcludeKeywords, movie.Directory);
        }

        public static Task ManualDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, TvShowEpisode episode) {
            var downloadable = (IDownloadable)episode;
            var directory = Novaroma.Helper.GetTvShowSeasonDirectory(engine.TvShowSeasonDirectoryTemplate, episode);
            return ManualDownload(engine, exceptionHandler, dialogService, episode, downloadable.GetSearchQuery(), downloadable.VideoQuality, downloadable.ExcludeKeywords, directory);
        }

        public static Task ManualDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService) {
            return ManualDownload(engine, exceptionHandler, dialogService, null, string.Empty, VideoQuality.Any, string.Empty, string.Empty);
        }

        private static Task ManualDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, IDownloadable downloadable,
                                                         string searchQuery, VideoQuality videoQuality, string excludeKeywords, string directory) {
            var viewModel = new DownloadSearchViewModel(engine, exceptionHandler, dialogService, downloadable, directory);
            var window = new DownloadSearchWindow(viewModel);

            var t = viewModel.InitSearch(searchQuery, videoQuality, excludeKeywords);
            window.ForceShow();
            return t;
        }

        public static Task ManualSubtitleDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, IDownloadable downloadable) {
            return ManualSubtitleDownload(engine, exceptionHandler, dialogService, new FileInfo(downloadable.FilePath), downloadable);
        }

        public static Task ManualSubtitleDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, FileInfo fileInfo) {
            return ManualSubtitleDownload(engine, exceptionHandler, dialogService, fileInfo, null);
        }

        private static Task ManualSubtitleDownload(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, FileInfo fileInfo, IDownloadable downloadable) {
            var searchQuery = downloadable != null ? downloadable.GetSearchQuery() : fileInfo.NameWithoutExtension();

            var viewModel = new SubtitleSearchViewModel(engine, exceptionHandler, dialogService, downloadable, fileInfo);
            var window = new SubtitleSearchWindow(viewModel);

            var t = viewModel.InitSearch(searchQuery);
            window.ForceShow();
            return t;
        }

        internal static void AllTvDownloadCheck(object prm, INovaromaEngine engine) {
            var tvShow = prm as TvShow;
            if (tvShow == null || !tvShow.AllBackgroundDownload.HasValue) return;

            tvShow.AllBackgroundSubtitleDownload = tvShow.AllBackgroundDownload.Value
                && engine.SubtitlesEnabled
                && (tvShow.Language == null || !engine.SubtitleLanguages.Contains(tvShow.Language.Value));
        }

        internal static void AllSeasonDownloadCheck(object prm, INovaromaEngine engine) {
            var season = prm as TvShowSeason;
            if (season == null || !season.AllBackgroundDownload.HasValue) return;

            var tvShow = season.TvShow;
            season.AllBackgroundSubtitleDownload = season.AllBackgroundDownload.Value && engine.SubtitlesEnabled
                && (tvShow.Language == null || !engine.SubtitleLanguages.Contains(tvShow.Language.Value));
        }

        internal static void EpisodeDownloadCheck(object prm, INovaromaEngine engine) {
            var episode = prm as TvShowEpisode;
            if (episode == null) return;

            var tvShow = episode.TvShowSeason.TvShow;
            episode.BackgroundSubtitleDownload = episode.BackgroundDownload && engine.SubtitlesEnabled
                && (tvShow.Language == null || !engine.SubtitleLanguages.Contains(tvShow.Language.Value));
        }

        internal static void MovieDownloadCheck(object prm, INovaromaEngine engine) {
            var movie = prm as Movie;
            if (movie == null) return;

            movie.BackgroundSubtitleDownload = movie.BackgroundDownload && engine.SubtitlesEnabled
                && (movie.Language == null || !engine.SubtitleLanguages.Contains(movie.Language.Value));
        }

        public static void ExitApplication() {
            IoCContainer.Resolve<IScheduler>().Shutdown(false);
            App.ServiceHost.Close();
            App.NotifyIcon.Dispose();
            Application.Current.Shutdown();
        }
    }
}
