<script lang="ts">
  import { onMount } from 'svelte';
  import { page as storesPage } from '$app/stores';
  import {
    getProducts,
    getProduct,
    getCategories,
    createProduct,
    updateProduct,
    deleteProduct
  } from '../../lib/api';
  import type {
    CategoryTreeItem,
    ProductListItem,
    ProductDetail,
    PagedResult
  } from '../../lib/types';
  import { Plus, Search, Filter, RefreshCw, Trash2, Edit, PackageSearch } from 'lucide-svelte';

  const defaultProducts: PagedResult<ProductListItem> = { items: [], pageSize: 12, totalCount: 0, cursor: null, nextCursor: null };
  let products = $state(defaultProducts);
  let categories = $state<CategoryTreeItem[]>([]);
  let activeCategoryOptions = $derived(
    categories.flatMap(c => {
      const flatten = (t: CategoryTreeItem[]): CategoryTreeItem[] => t.flatMap(i => [i, ...flatten(i.children)]);
      return [c, ...flatten(c.children)];
    }).filter(c => c.status === 'Active')
  );

  let searchInput = $state('');
  let appliedSearch = $state('');
  let categoryFilter = $state('');
  let priceFromFilter = $state('');
  let priceToFilter = $state('');
  let sortBy = $state('updatedAt');
  let sortDir = $state('desc');
  let pageSize = $state(12);
  let cursor = $state<string | null>(null);
  let cursorHistory = $state<(string | null)[]>([]);

  let productsBusy = $state(false);
  let productsError = $state('');

  let selectedProduct = $state<ProductDetail | null>(null);
  let isEditing = $state(false);
  let productForm = $state({
    id: null as number | null,
    rowVersion: '',
    name: '',
    description: '',
    price: '0.00',
    inventoryOnHand: '0',
    categoryId: '',
    primaryImageUrl: '',
    inventoryReason: '',
    changedBy: 'catalog-admin',
  });
  let productSaveBusy = $state(false);

  onMount(async () => {
    // Check if query params were passed from dashboard search
    const qs = $storesPage.url.searchParams;
    const initialSearch = qs.get('search');
    const initialId = qs.get('id');
    
    if (initialSearch) {
      searchInput = initialSearch;
      appliedSearch = initialSearch;
    }

    try {
      categories = await getCategories();
    } catch {}

    await refreshProducts();

    if (initialId) {
      void loadProduct(Number(initialId));
    }
  });

  async function refreshProducts() {
    productsBusy = true;
    productsError = '';
    try {
      products = await getProducts({
        query: appliedSearch || undefined,
        cursor,
        pageSize,
        categoryId: categoryFilter ? Number(categoryFilter) : null,
        priceFrom: priceFromFilter ? Number(priceFromFilter) : null,
        priceTo: priceToFilter ? Number(priceToFilter) : null,
        sortBy,
        sortDir,
      });
    } catch (error) {
      productsError = error instanceof Error ? error.message : 'Failed to fetch products';
    } finally {
      productsBusy = false;
    }
  }

  function applySearchFilter() {
    resetCursor();
    appliedSearch = searchInput.trim();
    void refreshProducts();
  }

  function resetCatalogFilters() {
    searchInput = '';
    appliedSearch = '';
    categoryFilter = '';
    priceFromFilter = '';
    priceToFilter = '';
    sortBy = 'updatedAt';
    sortDir = 'desc';
    pageSize = 12;
    resetCursor();
    void refreshProducts();
  }

  function resetCursor() {
    cursor = null;
    cursorHistory = [];
  }

  function goToNextPage() {
    if (!products.nextCursor) {
      return;
    }

    cursorHistory = [...cursorHistory, cursor];
    cursor = products.nextCursor;
    void refreshProducts();
  }

  function goToPreviousPage() {
    if (cursorHistory.length === 0) {
      return;
    }

    const nextHistory = [...cursorHistory];
    const previousCursor = nextHistory.pop() ?? null;
    cursorHistory = nextHistory;
    cursor = previousCursor;
    void refreshProducts();
  }

  async function loadProduct(id: number) {
    try {
      selectedProduct = await getProduct(id);
      isEditing = false;
    } catch (e) {
      console.error(e);
    }
  }

  function beginDrafting() {
    selectedProduct = null;
    isEditing = true;
    productForm = {
      id: null,
      rowVersion: '',
      name: '',
      description: '',
      price: '0.00',
      inventoryOnHand: '0',
      categoryId: activeCategoryOptions[0] ? String(activeCategoryOptions[0].id) : '',
      primaryImageUrl: '',
      inventoryReason: '',
      changedBy: 'catalog-admin',
    };
  }

  function editProduct(product: ProductDetail) {
    selectedProduct = product;
    isEditing = true;
    productForm = {
      id: product.id,
      rowVersion: product.rowVersion,
      name: product.name,
      description: product.description ?? '',
      price: product.price.toFixed(2),
      inventoryOnHand: String(product.inventoryOnHand),
      categoryId: String(product.categoryId),
      primaryImageUrl: product.primaryImageUrl ?? '',
      inventoryReason: '',
      changedBy: 'catalog-admin',
    };
  }

  async function submitProductForm(e: Event) {
    e.preventDefault();
    productSaveBusy = true;
    try {
      const payload = {
        name: productForm.name.trim(),
        description: productForm.description.trim() || null,
        price: Number(productForm.price),
        inventoryOnHand: Number(productForm.inventoryOnHand),
        categoryId: Number(productForm.categoryId),
        primaryImageUrl: productForm.primaryImageUrl.trim() || null,
        customAttributes: null,
        inventoryReason: productForm.inventoryReason.trim() || null,
        changedBy: productForm.changedBy.trim() || null,
      };

      let saved;
      if (productForm.id) {
        saved = await updateProduct(productForm.id, { ...payload, rowVersion: productForm.rowVersion });
      } else {
        saved = await createProduct(payload);
      }
      
      selectedProduct = saved;
      isEditing = false;
      await refreshProducts();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Error saving product');
    } finally {
      productSaveBusy = false;
    }
  }

  async function deleteSelected() {
    if (!productForm.id) return;
    if (!confirm(`Delete ${productForm.name}?`)) return;
    try {
      await deleteProduct(productForm.id);
      selectedProduct = null;
      isEditing = false;
      await refreshProducts();
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Error deleting product');
    }
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }
</script>

<svelte:head>
  <title>Products - Catalog Studio</title>
</svelte:head>

<div class="h-full flex flex-col xl:flex-row gap-6 animate-in fade-in duration-500">
  <!-- Left Column: Catalog Stream -->
  <div class="flex-1 flex flex-col gap-6 min-w-[60%]">
    <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h1 class="text-3xl font-bold tracking-tight text-white mb-1">Catalog Stream</h1>
        <p class="text-slate-400">Discover and curate your entire product collection.</p>
      </div>
      <button class="btn-primary flex items-center gap-2" onclick={beginDrafting}>
        <Plus size={18} /> New Product
      </button>
    </header>

    <!-- Filters Panel -->
    <div class="glass-panel p-6 space-y-4">
      <div class="flex flex-col sm:flex-row gap-4">
        <div class="relative flex-1">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
          <input 
            bind:value={searchInput}
            onkeydown={(e) => { if (e.key === 'Enter') applySearchFilter(); }}
            placeholder="Search products..."
            class="input-field pl-10"
          />
        </div>
        <button class="btn-ghost bg-slate-800/50 flex items-center gap-2" onclick={applySearchFilter}>
          Run Query
        </button>
      </div>
      
      <div class="grid grid-cols-2 md:grid-cols-5 gap-3">
        <select bind:value={categoryFilter} class="input-field text-sm" onchange={() => { resetCursor(); void refreshProducts(); }}>
          <option value="">All Categories</option>
          {#each activeCategoryOptions as cat}
            <option value={cat.id}>{cat.name}</option>
          {/each}
        </select>
        <select bind:value={sortBy} class="input-field text-sm" onchange={() => { resetCursor(); void refreshProducts(); }}>
          <option value="updatedAt">Latest</option>
          <option value="name">Name</option>
          <option value="price">Price</option>
          <option value="inventory">Inventory</option>
        </select>
        <select bind:value={sortDir} class="input-field text-sm" onchange={() => { resetCursor(); void refreshProducts(); }}>
          <option value="desc">Descending</option>
          <option value="asc">Ascending</option>
        </select>
        <input bind:value={priceFromFilter} placeholder="Min Price ($)" class="input-field text-sm" onchange={() => { resetCursor(); void refreshProducts(); }} />
        <input bind:value={priceToFilter} placeholder="Max Price ($)" class="input-field text-sm" onchange={() => { resetCursor(); void refreshProducts(); }} />
      </div>
      
      <div class="flex items-center justify-between mt-2 pt-2 border-t border-slate-800">
        <div class="flex items-center gap-2 text-sm text-slate-400">
          <Filter size={14} /> 
          {#if appliedSearch}<span>Query: {appliedSearch}</span>{/if}
          <span>{products.totalCount} results</span>
        </div>
        <button onclick={resetCatalogFilters} class="text-sm text-indigo-400 hover:text-indigo-300 transition-colors">
          Reset Filters
        </button>
      </div>
    </div>

    <!-- Product Grid -->
    <div class="flex-1 relative min-h-[400px]">
      {#if productsBusy}
        <div class="absolute inset-0 flex items-center justify-center bg-slate-950/50 backdrop-blur-sm z-10 rounded-xl">
          <RefreshCw class="animate-spin text-indigo-500" size={32} />
        </div>
      {/if}
      
      {#if productsError}
        <div class="p-4 bg-rose-500/10 text-rose-400 rounded-lg border border-rose-500/20">{productsError}</div>
      {:else if products.items.length === 0 && !productsBusy}
        <div class="h-full flex flex-col items-center justify-center text-slate-500 p-12 border border-dashed border-slate-800 rounded-2xl">
          <Search size={48} class="mb-4 opacity-50" />
          <h3 class="text-xl font-semibold text-slate-300">No products found</h3>
          <p class="mt-2 text-center max-w-sm">Try adjusting your filters or expanding your search criteria.</p>
        </div>
      {:else}
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {#each products.items as product}
            <button 
              class="glass-card text-left flex flex-col h-full overflow-hidden focus:outline-none focus:ring-2 focus:ring-indigo-500 {selectedProduct?.id === product.id ? 'ring-2 ring-indigo-500 bg-slate-800/80' : ''}"
              onclick={() => loadProduct(product.id)}
            >
              <div class="h-40 w-full bg-slate-900 flex items-center justify-center overflow-hidden border-b border-slate-800/50 relative">
                {#if product.primaryImageUrl}
                  <img src={product.primaryImageUrl} alt={product.name} class="w-full h-full object-cover transition-transform duration-500 hover:scale-110" loading="lazy" />
                {:else}
                  <span class="text-4xl font-bold text-slate-700">{product.name.charAt(0)}</span>
                {/if}
                <div class="absolute top-2 right-2 bg-slate-900/80 backdrop-blur-md px-2 py-1 rounded text-xs font-medium text-slate-300">
                  v{product.versionNumber}
                </div>
              </div>
              <div class="p-4 flex-1 flex flex-col">
                <div class="text-xs font-semibold text-indigo-400 mb-1">{product.categoryName}</div>
                <h3 class="text-lg font-bold text-white mb-2 leading-tight line-clamp-1">{product.name}</h3>
                <div class="mt-auto flex justify-between items-end pt-4">
                  <span class="text-lg font-bold text-cyan-400">{formatMoney(product.price)}</span>
                  <div class="flex items-center gap-1.5 text-xs text-slate-400">
                    <span class="w-2 h-2 rounded-full {product.inventoryOnHand > 10 ? 'bg-emerald-500' : product.inventoryOnHand > 0 ? 'bg-amber-500' : 'bg-rose-500'}"></span>
                    {product.inventoryOnHand} in stock
                  </div>
                </div>
              </div>
            </button>
          {/each}
        </div>
        
        <!-- Pagination -->
        <div class="mt-6 flex justify-between items-center glass-panel p-2 px-4 rounded-full">
          <button class="btn-ghost py-1.5 px-3 text-sm" disabled={cursorHistory.length === 0} onclick={goToPreviousPage}>Prev</button>
          <span class="text-sm font-medium text-slate-400">Showing <span class="text-slate-200">{products.items.length}</span> of {products.totalCount}</span>
          <button class="btn-ghost py-1.5 px-3 text-sm" disabled={!products.nextCursor} onclick={goToNextPage}>Next</button>
        </div>
      {/if}
    </div>
  </div>

  <!-- Right Column: Spotlight / Editor -->
  <div class="w-full xl:w-96 shrink-0 flex flex-col gap-6">
    {#if isEditing}
      <!-- Editor Panel -->
      <div class="glass-panel p-6 sticky top-8">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl font-bold text-white">{productForm.id ? 'Edit Product' : 'New Product'}</h2>
          {#if productForm.id}
            <button onclick={deleteSelected} class="p-2 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors">
              <Trash2 size={18} />
            </button>
          {/if}
        </div>
        
        <form onsubmit={submitProductForm} class="space-y-4">
          <div>
            <label class="block text-xs font-medium text-slate-400 mb-1">Name</label>
            <input bind:value={productForm.name} required class="input-field" placeholder="Aurora Lamp" />
          </div>
          <div>
            <label class="block text-xs font-medium text-slate-400 mb-1">Category</label>
            <select bind:value={productForm.categoryId} required class="input-field">
              {#each activeCategoryOptions as cat}
                <option value={cat.id}>{cat.name}</option>
              {/each}
            </select>
          </div>
          <div class="flex gap-4">
            <div class="flex-1">
              <label class="block text-xs font-medium text-slate-400 mb-1">Price ($)</label>
              <input type="number" step="0.01" bind:value={productForm.price} required class="input-field" />
            </div>
            <div class="flex-1">
              <label class="block text-xs font-medium text-slate-400 mb-1">Stock</label>
              <input type="number" bind:value={productForm.inventoryOnHand} required class="input-field" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-medium text-slate-400 mb-1">Description</label>
            <textarea bind:value={productForm.description} rows="3" class="input-field" placeholder="Product details..."></textarea>
          </div>
          <div>
            <label class="block text-xs font-medium text-slate-400 mb-1">Image URL</label>
            <input bind:value={productForm.primaryImageUrl} class="input-field" placeholder="https://..." />
          </div>
          
          <div class="pt-4 border-t border-slate-800 flex gap-3">
            <button type="button" class="btn-ghost flex-1" onclick={() => isEditing = false}>Cancel</button>
            <button type="submit" disabled={productSaveBusy} class="btn-primary flex-1">
              {productSaveBusy ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    {:else if selectedProduct}
      <!-- Spotlight Panel -->
      <div class="glass-panel overflow-hidden sticky top-8">
        <div class="h-64 bg-slate-900 relative">
          {#if selectedProduct.primaryImageUrl}
            <img src={selectedProduct.primaryImageUrl} alt={selectedProduct.name} class="w-full h-full object-cover" />
          {:else}
            <div class="w-full h-full flex items-center justify-center text-7xl font-bold text-slate-800 bg-slate-900 border-b border-slate-800/50">
              {selectedProduct.name.charAt(0)}
            </div>
          {/if}
          <div class="absolute inset-0 bg-gradient-to-t from-slate-950 via-transparent to-transparent"></div>
          
          <button onclick={() => editProduct(selectedProduct!)} class="absolute top-4 right-4 p-2 bg-slate-900/50 hover:bg-indigo-500 text-white backdrop-blur-md rounded-full shadow-lg transition-all">
            <Edit size={18} />
          </button>
        </div>
        
        <div class="p-6">
          <div class="inline-block px-3 py-1 mb-3 rounded-full bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 text-xs font-bold uppercase tracking-wider">
            {selectedProduct.categoryName}
          </div>
          <h2 class="text-2xl font-bold text-white mb-2">{selectedProduct.name}</h2>
          <p class="text-slate-400 text-sm mb-6 leading-relaxed">
            {selectedProduct.description || 'No description available for this product.'}
          </p>
          
          <div class="grid grid-cols-2 gap-4 mb-6">
            <div class="p-4 rounded-xl bg-slate-900/50 border border-slate-800">
              <div class="text-xs text-slate-500 mb-1 font-medium">Price</div>
              <div class="text-xl font-bold text-cyan-400">{formatMoney(selectedProduct.price)}</div>
            </div>
            <div class="p-4 rounded-xl bg-slate-900/50 border border-slate-800">
              <div class="text-xs text-slate-500 mb-1 font-medium">Inventory</div>
              <div class="text-xl font-bold {selectedProduct.inventoryOnHand > 0 ? 'text-emerald-400' : 'text-rose-400'}">{selectedProduct.inventoryOnHand} Units</div>
            </div>
          </div>
          
          <div class="text-xs text-slate-500 flex justify-between pt-4 border-t border-slate-800">
            <span>v{selectedProduct.versionNumber}</span>
            <span>Created {new Date(selectedProduct.createdAtUtc).toLocaleDateString()}</span>
          </div>
        </div>
      </div>
    {:else}
      <!-- Empty Spotlight State -->
      <div class="glass-panel p-8 h-64 flex flex-col items-center justify-center text-center text-slate-500 border border-dashed border-slate-800 sticky top-8">
        <PackageSearch size={48} class="mb-4 opacity-50" />
        <h3 class="text-lg font-medium text-slate-300">Spotlight</h3>
        <p class="text-sm mt-2 max-w-[200px]">Select a product from the stream to view its details or edit properties.</p>
      </div>
    {/if}
  </div>
</div>
