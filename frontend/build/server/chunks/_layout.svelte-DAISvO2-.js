import { c as attr_class, d as stringify, f as store_get, h as slot, j as unsubscribe_stores, k as sanitize_props, l as spread_props, m as getContext } from './index2-CRKCN0LV.js';
import './root-y1zgPQ5o.js';
import './state.svelte-zEazp7z2.js';
import { I as Icon } from './Icon-DIKcpDH_.js';
import { P as Package_search } from './package-search-DbvPVVh-.js';
import { F as Folders } from './folders-CezkxB5d.js';

const getStores = () => {
  const stores$1 = getContext("__svelte__");
  return {
    /** @type {typeof page} */
    page: {
      subscribe: stores$1.page.subscribe
    },
    /** @type {typeof navigating} */
    navigating: {
      subscribe: stores$1.navigating.subscribe
    },
    /** @type {typeof updated} */
    updated: stores$1.updated
  };
};
const page = {
  subscribe(fn) {
    const store = getStores().page;
    return store.subscribe(fn);
  }
};
function Layout_dashboard($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    [
      "rect",
      { "width": "7", "height": "9", "x": "3", "y": "3", "rx": "1" }
    ],
    [
      "rect",
      { "width": "7", "height": "5", "x": "14", "y": "3", "rx": "1" }
    ],
    [
      "rect",
      { "width": "7", "height": "9", "x": "14", "y": "12", "rx": "1" }
    ],
    [
      "rect",
      { "width": "7", "height": "5", "x": "3", "y": "16", "rx": "1" }
    ]
  ];
  Icon($$renderer, spread_props([
    { name: "layout-dashboard" },
    $$sanitized_props,
    {
      /**
       * @component @name LayoutDashboard
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cmVjdCB3aWR0aD0iNyIgaGVpZ2h0PSI5IiB4PSIzIiB5PSIzIiByeD0iMSIgLz4KICA8cmVjdCB3aWR0aD0iNyIgaGVpZ2h0PSI1IiB4PSIxNCIgeT0iMyIgcng9IjEiIC8+CiAgPHJlY3Qgd2lkdGg9IjciIGhlaWdodD0iOSIgeD0iMTQiIHk9IjEyIiByeD0iMSIgLz4KICA8cmVjdCB3aWR0aD0iNyIgaGVpZ2h0PSI1IiB4PSIzIiB5PSIxNiIgcng9IjEiIC8+Cjwvc3ZnPgo=) - https://lucide.dev/icons/layout-dashboard
       * @see https://lucide.dev/guide/packages/lucide-svelte - Documentation
       *
       * @param {Object} props - Lucide icons props and any valid SVG attribute
       * @returns {FunctionalComponent} Svelte component
       *
       */
      iconNode,
      children: ($$renderer2) => {
        $$renderer2.push(`<!--[-->`);
        slot($$renderer2, $$props, "default", {});
        $$renderer2.push(`<!--]-->`);
      },
      $$slots: { default: true }
    }
  ]));
}
function _layout($$renderer, $$props) {
  $$renderer.component(($$renderer2) => {
    var $$store_subs;
    $$renderer2.push(`<div class="flex h-screen bg-slate-950 text-slate-100 overflow-hidden font-sans"><aside class="w-64 border-r border-slate-800/50 bg-slate-900/50 backdrop-blur-xl flex flex-col z-20 shrink-0"><div class="p-6 border-b border-slate-800/50"><h1 class="text-xl font-bold bg-gradient-to-r from-indigo-400 to-cyan-400 bg-clip-text text-transparent tracking-tight">Catalog Studio</h1></div> <nav class="p-4 flex-1 space-y-2 overflow-y-auto"><a href="/"${attr_class(`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 ${stringify(store_get($$store_subs ??= {}, "$page", page).url.pathname === "/" ? "bg-indigo-500/10 text-indigo-400 shadow-sm" : "text-slate-400 hover:text-slate-100 hover:bg-slate-800/50")}`)}>`);
    Layout_dashboard($$renderer2, { size: 20, strokeWidth: 2.5 });
    $$renderer2.push(`<!----> <span class="font-medium">Dashboard</span></a> <a href="/products"${attr_class(`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 ${stringify(store_get($$store_subs ??= {}, "$page", page).url.pathname.startsWith("/products") ? "bg-indigo-500/10 text-indigo-400 shadow-sm" : "text-slate-400 hover:text-slate-100 hover:bg-slate-800/50")}`)}>`);
    Package_search($$renderer2, { size: 20, strokeWidth: 2.5 });
    $$renderer2.push(`<!----> <span class="font-medium">Products</span></a> <a href="/categories"${attr_class(`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 ${stringify(store_get($$store_subs ??= {}, "$page", page).url.pathname.startsWith("/categories") ? "bg-indigo-500/10 text-indigo-400 shadow-sm" : "text-slate-400 hover:text-slate-100 hover:bg-slate-800/50")}`)}>`);
    Folders($$renderer2, { size: 20, strokeWidth: 2.5 });
    $$renderer2.push(`<!----> <span class="font-medium">Categories</span></a></nav> <div class="p-4 border-t border-slate-800/50 flex items-center gap-3 text-sm text-slate-400"><div class="relative flex h-3 w-3"><span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span> <span class="relative inline-flex rounded-full h-3 w-3 bg-emerald-500"></span></div> <span class="font-medium tracking-wide">System Online</span></div></aside> <main class="flex-1 overflow-auto bg-slate-950 relative"><div class="absolute inset-0 opacity-[0.03] pointer-events-none" style="background-image: url('data:image/svg+xml,%3Csvg viewBox=%220 0 200 200%22 xmlns=%22http://www.w3.org/2000/svg%22%3E%3Cfilter id=%22noiseFilter%22%3E%3CfeTurbulence type=%22fractalNoise%22 baseFrequency=%220.65%22 numOctaves=%223%22 stitchTiles=%22stitch%22/%3E%3C/filter%3E%3Crect width=%22100%25%22 height=%22100%25%22 filter=%22url(%23noiseFilter)%22/%3E%3C/svg%3E');"></div> <div class="absolute top-0 left-1/4 w-96 h-96 bg-indigo-500/10 rounded-full blur-[128px] pointer-events-none"></div> <div class="absolute bottom-0 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-[128px] pointer-events-none"></div> <div class="relative z-10 w-full h-full p-8 max-w-[1600px] mx-auto"><!--[-->`);
    slot($$renderer2, $$props, "default", {});
    $$renderer2.push(`<!--]--></div></main></div>`);
    if ($$store_subs) unsubscribe_stores($$store_subs);
  });
}

export { _layout as default };
//# sourceMappingURL=_layout.svelte-DAISvO2-.js.map
