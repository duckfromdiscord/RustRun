using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.RustInterop
{
    internal class RustMethods
    {

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe SearchResults init_search(ushort* utf16_str, int utf16_len);

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe void drop_search(SearchResults srs);

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe void drop_search_result(CSearchResult csr);


        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe ContextMenuResults get_context_menu(CSSearchResult cssr);

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe void drop_context_menu_result(CContextMenuResult cs);

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe void drop_context_menu(ContextMenuResults css);




        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe string* get_plugin_info(byte which);

        [DllImport("rustinteroprust.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern unsafe void free_c_string(string* str);

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe partial struct CSearchResult
        {
            public string* query_text_display;
            public string* ico_path;
            public string* title;
            public string* subtitle;
            public string* tooltip_a;
            public string* tooltip_b;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe partial struct CContextMenuResult
        {
            public string* plugin_name;
            public string* title;
            public string* font_family;
            public string* glyph;
            public int accelerator_key;
            public int accelerator_modifiers;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe partial struct SearchResults
        {
            public nuint len;
            public CSearchResult* ptr;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal unsafe partial struct CSSearchResult
        {
            public int query_text_display_length;
            public ushort* query_text_display;
            public int ico_path_length;
            public ushort* ico_path;
            public int title_length;
            public ushort* title;
            public int subtitle_length;
            public ushort* subtitle;
            public int tooltip_a_length;
            public ushort* tooltip_a;
            public int tooltip_b_length;
            public ushort* tooltip_b;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe partial struct ContextMenuResults
        {
            public nuint len;
            public CContextMenuResult* ptr;
        }

        public unsafe static String TakeString(string* cString, bool base64)
        {
            var str = new String((sbyte*)cString);
            free_c_string(cString);
            if (base64)
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            return str;
        }
        public unsafe static String CastString(string* cString, bool base64)
        {
            var str = new String((sbyte*)cString);
            if (base64)
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            return str;
        }

#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    }
}
