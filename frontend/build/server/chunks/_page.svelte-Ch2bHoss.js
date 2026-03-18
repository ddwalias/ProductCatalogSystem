import { q as head, r as attr, t as ensure_array_like, n as escape_html, c as attr_class, d as stringify, x as derived, k as sanitize_props, l as spread_props, h as slot } from './index2-CRKCN0LV.js';
import './root-y1zgPQ5o.js';
import './state.svelte-zEazp7z2.js';
import { P as Plus } from './plus-Cbu_0YdE.js';
import { S as Search } from './search-DV1unkTP.js';
import { I as Icon } from './Icon-DIKcpDH_.js';
import { P as Package_search } from './package-search-DbvPVVh-.js';

function Funnel($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    [
      "path",
      {
        "d": "M10 20a1 1 0 0 0 .553.895l2 1A1 1 0 0 0 14 21v-7a2 2 0 0 1 .517-1.341L21.74 4.67A1 1 0 0 0 21 3H3a1 1 0 0 0-.742 1.67l7.225 7.989A2 2 0 0 1 10 14z"
      }
    ]
  ];
  Icon($$renderer, spread_props([
    { name: "funnel" },
    $$sanitized_props,
    {
      /**
       * @component @name Funnel
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cGF0aCBkPSJNMTAgMjBhMSAxIDAgMCAwIC41NTMuODk1bDIgMUExIDEgMCAwIDAgMTQgMjF2LTdhMiAyIDAgMCAxIC41MTctMS4zNDFMMjEuNzQgNC42N0ExIDEgMCAwIDAgMjEgM0gzYTEgMSAwIDAgMC0uNzQyIDEuNjdsNy4yMjUgNy45ODlBMiAyIDAgMCAxIDEwIDE0eiIgLz4KPC9zdmc+Cg==) - https://lucide.dev/icons/funnel
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
function Refresh_cw($$renderer, $$props) {
  const $$sanitized_props = sanitize_props($$props);
  const iconNode = [
    [
      "path",
      { "d": "M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8" }
    ],
    ["path", { "d": "M21 3v5h-5" }],
    [
      "path",
      { "d": "M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16" }
    ],
    ["path", { "d": "M8 16H3v5" }]
  ];
  Icon($$renderer, spread_props([
    { name: "refresh-cw" },
    $$sanitized_props,
    {
      /**
       * @component @name RefreshCw
       * @description Lucide SVG icon component, renders SVG Element with children.
       *
       * @preview ![img](data:image/svg+xml;base64,PHN2ZyAgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIgogIHdpZHRoPSIyNCIKICBoZWlnaHQ9IjI0IgogIHZpZXdCb3g9IjAgMCAyNCAyNCIKICBmaWxsPSJub25lIgogIHN0cm9rZT0iIzAwMCIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6ICNmZmY7IGJvcmRlci1yYWRpdXM6IDJweCIKICBzdHJva2Utd2lkdGg9IjIiCiAgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIgogIHN0cm9rZS1saW5lam9pbj0icm91bmQiCj4KICA8cGF0aCBkPSJNMyAxMmE5IDkgMCAwIDEgOS05IDkuNzUgOS43NSAwIDAgMSA2Ljc0IDIuNzRMMjEgOCIgLz4KICA8cGF0aCBkPSJNMjEgM3Y1aC01IiAvPgogIDxwYXRoIGQ9Ik0yMSAxMmE5IDkgMCAwIDEtOSA5IDkuNzUgOS43NSAwIDAgMS02Ljc0LTIuNzRMMyAxNiIgLz4KICA8cGF0aCBkPSJNOCAxNkgzdjUiIC8+Cjwvc3ZnPgo=) - https://lucide.dev/icons/refresh-cw
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
class ApiError extends Error {
  status;
  errors;
  constructor(status, message, errors = null) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}
function buildQuery(params) {
  const searchParams = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== null && value !== void 0 && value !== "") {
      searchParams.set(key, String(value));
    }
  });
  const query = searchParams.toString();
  return query.length > 0 ? `?${query}` : "";
}
async function request(path, init) {
  const response = await fetch(path, {
    headers: {
      Accept: "application/json",
      ...{},
      ...init?.headers
    },
    ...init
  });
  if (!response.ok) {
    const fallbackMessage = `Request failed with status ${response.status}.`;
    try {
      const problem = await response.json();
      throw new ApiError(
        response.status,
        problem.detail ?? problem.title ?? fallbackMessage,
        problem.errors ?? null
      );
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      throw new ApiError(response.status, fallbackMessage);
    }
  }
  if (response.status === 204) {
    return void 0;
  }
  return await response.json();
}
function getProducts(params) {
  return request(
    `/api/products${buildQuery(params)}`
  );
}
function _page($$renderer, $$props) {
  $$renderer.component(($$renderer2) => {
    const defaultProducts = {
      items: [],
      pageSize: 12,
      totalCount: 0,
      cursor: null,
      nextCursor: null
    };
    let products = defaultProducts;
    let categories = [];
    let activeCategoryOptions = derived(() => categories.flatMap((c) => {
      const flatten = (t) => t.flatMap((i) => [i, ...flatten(i.children)]);
      return [c, ...flatten(c.children)];
    }).filter((c) => c.status === "Active"));
    let searchInput = "";
    let appliedSearch = "";
    let categoryFilter = "";
    let priceFromFilter = "";
    let priceToFilter = "";
    let sortBy = "updatedAt";
    let sortDir = "desc";
    let pageSize = 12;
    let cursor = null;
    let cursorHistory = [];
    let productsBusy = false;
    let productsError = "";
    let selectedProduct = null;
    async function refreshProducts() {
      productsBusy = true;
      productsError = "";
      try {
        products = await getProducts({
          query: appliedSearch || void 0,
          cursor,
          pageSize,
          categoryId: categoryFilter ? Number(categoryFilter) : null,
          priceFrom: priceFromFilter ? Number(priceFromFilter) : null,
          priceTo: priceToFilter ? Number(priceToFilter) : null,
          sortBy,
          sortDir
        });
      } catch (error) {
        productsError = error instanceof Error ? error.message : "Failed to fetch products";
      } finally {
        productsBusy = false;
      }
    }
    function resetCursor() {
      cursor = null;
      cursorHistory = [];
    }
    function formatMoney(value) {
      return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(value);
    }
    head("1dj9mz1", $$renderer2, ($$renderer3) => {
      $$renderer3.title(($$renderer4) => {
        $$renderer4.push(`<title>Products - Catalog Studio</title>`);
      });
    });
    $$renderer2.push(`<div class="h-full flex flex-col xl:flex-row gap-6 animate-in fade-in duration-500"><div class="flex-1 flex flex-col gap-6 min-w-[60%]"><header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"><div><h1 class="text-3xl font-bold tracking-tight text-white mb-1">Catalog Stream</h1> <p class="text-slate-400">Discover and curate your entire product collection.</p></div> <button class="btn-primary flex items-center gap-2">`);
    Plus($$renderer2, { size: 18 });
    $$renderer2.push(`<!----> New Product</button></header> <div class="glass-panel p-6 space-y-4"><div class="flex flex-col sm:flex-row gap-4"><div class="relative flex-1">`);
    Search($$renderer2, {
      class: "absolute left-3 top-1/2 -translate-y-1/2 text-slate-400",
      size: 18
    });
    $$renderer2.push(`<!----> <input${attr("value", searchInput)} placeholder="Search products..." class="input-field pl-10"/></div> <button class="btn-ghost bg-slate-800/50 flex items-center gap-2">Run Query</button></div> <div class="grid grid-cols-2 md:grid-cols-5 gap-3">`);
    $$renderer2.select(
      {
        value: categoryFilter,
        class: "input-field text-sm",
        onchange: () => {
          resetCursor();
          void refreshProducts();
        }
      },
      ($$renderer3) => {
        $$renderer3.option({ value: "" }, ($$renderer4) => {
          $$renderer4.push(`All Categories`);
        });
        $$renderer3.push(`<!--[-->`);
        const each_array = ensure_array_like(activeCategoryOptions());
        for (let $$index = 0, $$length = each_array.length; $$index < $$length; $$index++) {
          let cat = each_array[$$index];
          $$renderer3.option({ value: cat.id }, ($$renderer4) => {
            $$renderer4.push(`${escape_html(cat.name)}`);
          });
        }
        $$renderer3.push(`<!--]-->`);
      }
    );
    $$renderer2.push(` `);
    $$renderer2.select(
      {
        value: sortBy,
        class: "input-field text-sm",
        onchange: () => {
          resetCursor();
          void refreshProducts();
        }
      },
      ($$renderer3) => {
        $$renderer3.option({ value: "updatedAt" }, ($$renderer4) => {
          $$renderer4.push(`Latest`);
        });
        $$renderer3.option({ value: "name" }, ($$renderer4) => {
          $$renderer4.push(`Name`);
        });
        $$renderer3.option({ value: "price" }, ($$renderer4) => {
          $$renderer4.push(`Price`);
        });
        $$renderer3.option({ value: "inventory" }, ($$renderer4) => {
          $$renderer4.push(`Inventory`);
        });
      }
    );
    $$renderer2.push(` `);
    $$renderer2.select(
      {
        value: sortDir,
        class: "input-field text-sm",
        onchange: () => {
          resetCursor();
          void refreshProducts();
        }
      },
      ($$renderer3) => {
        $$renderer3.option({ value: "desc" }, ($$renderer4) => {
          $$renderer4.push(`Descending`);
        });
        $$renderer3.option({ value: "asc" }, ($$renderer4) => {
          $$renderer4.push(`Ascending`);
        });
      }
    );
    $$renderer2.push(` <input${attr("value", priceFromFilter)} placeholder="Min Price ($)" class="input-field text-sm"/> <input${attr("value", priceToFilter)} placeholder="Max Price ($)" class="input-field text-sm"/></div> <div class="flex items-center justify-between mt-2 pt-2 border-t border-slate-800"><div class="flex items-center gap-2 text-sm text-slate-400">`);
    Funnel($$renderer2, { size: 14 });
    $$renderer2.push(`<!----> `);
    {
      $$renderer2.push("<!--[-1-->");
    }
    $$renderer2.push(`<!--]--> <span>${escape_html(products.totalCount)} results</span></div> <button class="text-sm text-indigo-400 hover:text-indigo-300 transition-colors">Reset Filters</button></div></div> <div class="flex-1 relative min-h-[400px]">`);
    if (productsBusy) {
      $$renderer2.push("<!--[0-->");
      $$renderer2.push(`<div class="absolute inset-0 flex items-center justify-center bg-slate-950/50 backdrop-blur-sm z-10 rounded-xl">`);
      Refresh_cw($$renderer2, { class: "animate-spin text-indigo-500", size: 32 });
      $$renderer2.push(`<!----></div>`);
    } else {
      $$renderer2.push("<!--[-1-->");
    }
    $$renderer2.push(`<!--]--> `);
    if (productsError) {
      $$renderer2.push("<!--[0-->");
      $$renderer2.push(`<div class="p-4 bg-rose-500/10 text-rose-400 rounded-lg border border-rose-500/20">${escape_html(productsError)}</div>`);
    } else if (products.items.length === 0 && !productsBusy) {
      $$renderer2.push("<!--[1-->");
      $$renderer2.push(`<div class="h-full flex flex-col items-center justify-center text-slate-500 p-12 border border-dashed border-slate-800 rounded-2xl">`);
      Search($$renderer2, { size: 48, class: "mb-4 opacity-50" });
      $$renderer2.push(`<!----> <h3 class="text-xl font-semibold text-slate-300">No products found</h3> <p class="mt-2 text-center max-w-sm">Try adjusting your filters or expanding your search criteria.</p></div>`);
    } else {
      $$renderer2.push("<!--[-1-->");
      $$renderer2.push(`<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"><!--[-->`);
      const each_array_1 = ensure_array_like(products.items);
      for (let $$index_1 = 0, $$length = each_array_1.length; $$index_1 < $$length; $$index_1++) {
        let product = each_array_1[$$index_1];
        $$renderer2.push(`<button${attr_class(`glass-card text-left flex flex-col h-full overflow-hidden focus:outline-none focus:ring-2 focus:ring-indigo-500 ${stringify(selectedProduct?.id === product.id ? "ring-2 ring-indigo-500 bg-slate-800/80" : "")}`)}><div class="h-40 w-full bg-slate-900 flex items-center justify-center overflow-hidden border-b border-slate-800/50 relative">`);
        if (product.primaryImageUrl) {
          $$renderer2.push("<!--[0-->");
          $$renderer2.push(`<img${attr("src", product.primaryImageUrl)}${attr("alt", product.name)} class="w-full h-full object-cover transition-transform duration-500 hover:scale-110" loading="lazy"/>`);
        } else {
          $$renderer2.push("<!--[-1-->");
          $$renderer2.push(`<span class="text-4xl font-bold text-slate-700">${escape_html(product.name.charAt(0))}</span>`);
        }
        $$renderer2.push(`<!--]--> <div class="absolute top-2 right-2 bg-slate-900/80 backdrop-blur-md px-2 py-1 rounded text-xs font-medium text-slate-300">v${escape_html(product.versionNumber)}</div></div> <div class="p-4 flex-1 flex flex-col"><div class="text-xs font-semibold text-indigo-400 mb-1">${escape_html(product.categoryName)}</div> <h3 class="text-lg font-bold text-white mb-2 leading-tight line-clamp-1">${escape_html(product.name)}</h3> <div class="mt-auto flex justify-between items-end pt-4"><span class="text-lg font-bold text-cyan-400">${escape_html(formatMoney(product.price))}</span> <div class="flex items-center gap-1.5 text-xs text-slate-400"><span${attr_class(`w-2 h-2 rounded-full ${stringify(product.inventoryOnHand > 10 ? "bg-emerald-500" : product.inventoryOnHand > 0 ? "bg-amber-500" : "bg-rose-500")}`)}></span> ${escape_html(product.inventoryOnHand)} in stock</div></div></div></button>`);
      }
      $$renderer2.push(`<!--]--></div> <div class="mt-6 flex justify-between items-center glass-panel p-2 px-4 rounded-full"><button class="btn-ghost py-1.5 px-3 text-sm"${attr("disabled", cursorHistory.length === 0, true)}>Prev</button> <span class="text-sm font-medium text-slate-400">Showing <span class="text-slate-200">${escape_html(products.items.length)}</span> of ${escape_html(products.totalCount)}</span> <button class="btn-ghost py-1.5 px-3 text-sm"${attr("disabled", !products.nextCursor, true)}>Next</button></div>`);
    }
    $$renderer2.push(`<!--]--></div></div> <div class="w-full xl:w-96 shrink-0 flex flex-col gap-6">`);
    {
      $$renderer2.push("<!--[-1-->");
      $$renderer2.push(`<div class="glass-panel p-8 h-64 flex flex-col items-center justify-center text-center text-slate-500 border border-dashed border-slate-800 sticky top-8">`);
      Package_search($$renderer2, { size: 48, class: "mb-4 opacity-50" });
      $$renderer2.push(`<!----> <h3 class="text-lg font-medium text-slate-300">Spotlight</h3> <p class="text-sm mt-2 max-w-[200px]">Select a product from the stream to view its details or edit properties.</p></div>`);
    }
    $$renderer2.push(`<!--]--></div></div>`);
  });
}

export { _page as default };
//# sourceMappingURL=_page.svelte-Ch2bHoss.js.map
