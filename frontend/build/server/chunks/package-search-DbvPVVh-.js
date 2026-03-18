import { k as sanitize_props, l as spread_props, h as slot } from './index2-CRKCN0LV.js';
import { I as Icon } from './Icon-DIKcpDH_.js';

function Package_search($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    ["path", { "d": "M12 22V12" }],
    ["path", { "d": "M20.27 18.27 22 20" }],
    [
      "path",
      {
        "d": "M21 10.498V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.729l7 4a2 2 0 0 0 2 .001l.98-.559"
      }
    ],
    ["path", { "d": "M3.29 7 12 12l8.71-5" }],
    ["path", { "d": "m7.5 4.27 8.997 5.148" }],
    ["circle", { "cx": "18.5", "cy": "16.5", "r": "2.5" }]
  ];
  Icon($$renderer, spread_props([
    { name: "package-search" },
    $$sanitized_props,
    {
      /**
       * @component @name PackageSearch
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cGF0aCBkPSJNMTIgMjJWMTIiIC8+CiAgPHBhdGggZD0iTTIwLjI3IDE4LjI3IDIyIDIwIiAvPgogIDxwYXRoIGQ9Ik0yMSAxMC40OThWOGEyIDIgMCAwIDAtMS0xLjczbC03LTRhMiAyIDAgMCAwLTIgMGwtNyA0QTIgMiAwIDAgMCAzIDh2OGEyIDIgMCAwIDAgMSAxLjcyOWw3IDRhMiAyIDAgMCAwIDIgLjAwMWwuOTgtLjU1OSIgLz4KICA8cGF0aCBkPSJNMy4yOSA3IDEyIDEybDguNzEtNSIgLz4KICA8cGF0aCBkPSJtNy41IDQuMjcgOC45OTcgNS4xNDgiIC8+CiAgPGNpcmNsZSBjeD0iMTguNSIgY3k9IjE2LjUiIHI9IjIuNSIgLz4KPC9zdmc+Cg==) - https://lucide.dev/icons/package-search
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

export { Package_search as P };
//# sourceMappingURL=package-search-DbvPVVh-.js.map
