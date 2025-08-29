using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using ManagedCommon;
using Wox.Plugin;
using Wox.Plugin.Common.Win32;
using static Community.PowerToys.Run.Plugin.RustInterop.RustMethods;

namespace Community.PowerToys.Run.Plugin.RustInterop
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {


        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static String PluginID
        {
            get
            {
                unsafe
                {
                    return RustMethods.TakeString(RustMethods.get_plugin_info(0));
                }
            }
        }

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name
        {
            get
            {
                unsafe
                {
                    return RustMethods.TakeString(RustMethods.get_plugin_info(1));
                }
            }
        }

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description
        {
            get
            {
                unsafe
                {
                    return RustMethods.TakeString(RustMethods.get_plugin_info(2));
                }
            }
        }

        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {

            var search = query.Search;
            List<Result> results = [];

            unsafe
            {
                fixed (char* p = search)
                {

                    SearchResults x = RustMethods.init_search((ushort*)p, search.Length);
                    // full copy: https://stackoverflow.com/a/62223041
                    for (nuint i = 0; i < x.len; i++)
                    {
                        RustMethods.CSearchResult csr = x.ptr[i];
                        results.Add(new Result
                        {
                            QueryTextDisplay = RustMethods.CastString(csr.query_text_display),
                            IcoPath = RustMethods.CastString(csr.ico_path),
                            Title = RustMethods.CastString(csr.title),
                            SubTitle = RustMethods.CastString(csr.subtitle),
                            ToolTipData = new ToolTipData(RustMethods.CastString(csr.tooltip_a), RustMethods.CastString(csr.tooltip_b)),
                            Action = _ =>
                            {
                                return true;
                            },
                            ContextData = search,
                        });
                        drop_search_result(csr);
                    }
                    drop_search(x);
                }
            }

            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string search)
            {
                List<ContextMenuResult> results = [];
                unsafe
                {
                    ContextMenuResults x;
                    // why C#. why.
                    fixed (char* query_text_display = selectedResult.QueryTextDisplay)
                    {
                        fixed (char* ico_path = selectedResult.IcoPath)
                        {
                            fixed (char* title = selectedResult.Title)
                            {
                                fixed (char* subtitle = selectedResult.SubTitle)
                                {
                                    fixed (char* tooltip_a = selectedResult.ToolTipData.Title)
                                    {
                                        fixed (char* tooltip_b = selectedResult.ToolTipData.Text)
                                        {
                                            x = RustMethods.get_context_menu(new CSSearchResult
                                            {
                                                query_text_display_length = selectedResult.QueryTextDisplay.Length,
                                                query_text_display = (ushort*)query_text_display,
                                                ico_path_length = selectedResult.IcoPath.Length,
                                                ico_path = (ushort*)ico_path,
                                                title_length = selectedResult.QueryTextDisplay.Length,
                                                title = (ushort*)query_text_display,
                                                subtitle_length = selectedResult.QueryTextDisplay.Length,
                                                subtitle = (ushort*)query_text_display,
                                                tooltip_a_length = selectedResult.ToolTipData.Title.Length,
                                                tooltip_a = (ushort*)tooltip_a,
                                                tooltip_b_length = selectedResult.ToolTipData.Text.Length,
                                                tooltip_b = (ushort*)tooltip_b,
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // full copy: https://stackoverflow.com/a/62223041
                    for (nuint i = 0; i < x.len; i++)
                    {
                        RustMethods.CContextMenuResult csr = x.ptr[i];
                        results.Add(new ContextMenuResult
                        {
                            PluginName = RustMethods.CastString(csr.plugin_name),
                            Title = RustMethods.CastString(csr.title),
                            FontFamily = RustMethods.CastString(csr.font_family),
                            Glyph = RustMethods.CastString(csr.glyph),
                            AcceleratorKey = (Key)csr.accelerator_key,
                            AcceleratorModifiers = (ModifierKeys)csr.accelerator_modifiers,
                            Action = _ =>
                            {
                                return true;
                            }
                        });
                        drop_context_menu_result(csr);
                    }
                    drop_context_menu(x);
                }
                return results;
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/rustinterop.light.png" : "Images/rustinterop.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
