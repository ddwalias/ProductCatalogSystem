<script lang="ts">
  import { onMount } from 'svelte';
  import {
    getCategories,
    getCategoryProducts,
    createCategory,
    updateCategory
  } from '../../lib/api';
  import CategoryNode from '../../lib/CategoryNode.svelte';
  import type { CategoryTreeItem, CategoryWritePayload, CategoryUpdatePayload, PagedResult, ProductListItem } from '../../lib/types';
  import { Folders, Plus, Layers, RefreshCw, BarChart } from 'lucide-svelte';

  let categories = $state<CategoryTreeItem[]>([]);
  let categoriesBusy = $state(false);
  let focusedCategoryId = $state<number | null>(null);
  let focusedCategory = $derived(findCategoryById(categories, focusedCategoryId));
  let categoryOptions = $derived(flattenCategories(categories));
  
  let categoryProducts = $state<PagedResult<ProductListItem>>({ items: [], pageSize: 6, totalCount: 0, cursor: null, nextCursor: null });
  let categoryProductsBusy = $state(false);

  let categoryForm = $state({
    id: null as number | null,
    rowVersion: '',
    name: '',
    description: '',
    parentCategoryId: '',
    status: 'Active' as 'Active' | 'Inactive',
    displayOrder: '10',
  });
  let categorySaveBusy = $state(false);
  let categoryMessage = $state('');
  let isError = $state(false);

  onMount(() => {
    void refreshCategories();
  });

  $effect(() => {
    if (focusedCategoryId) void refreshCategoryProducts();
  });

  async function refreshCategories() {
    categoriesBusy = true;
    try {
      const nextCategories = await getCategories();
      categories = nextCategories;

      // Select first category automatically if nothing is selected
      if (!focusedCategoryId && nextCategories.length > 0) {
        focusCategory(nextCategories[0]);
      } else if (categoryForm.id) {
        const refreshedCategory = findCategoryById(nextCategories, categoryForm.id);
        if (refreshedCategory) {
          categoryForm = categoryToForm(refreshedCategory);
        }
      }
    } catch (error) {
      console.error(error);
    } finally {
      categoriesBusy = false;
    }
  }

  async function refreshCategoryProducts() {
    if (!focusedCategoryId) return;
    categoryProductsBusy = true;
    try {
      categoryProducts = await getCategoryProducts(focusedCategoryId, {
        pageSize: 6,
        sortBy: 'updatedAt',
        sortDir: 'desc',
      });
    } catch (error) {
      console.error(error);
    } finally {
      categoryProductsBusy = false;
    }
  }

  function focusCategory(category: CategoryTreeItem) {
    focusedCategoryId = category.id;
    categoryForm = categoryToForm(category);
    categoryMessage = `Focused on ${category.name}`;
    isError = false;
  }

  function beginCreateCategory(parentCategoryId: number | null = null) {
    categoryForm = {
      id: null,
      rowVersion: '',
      name: '',
      description: '',
      parentCategoryId: parentCategoryId ? String(parentCategoryId) : '',
      status: 'Active',
      displayOrder: '10',
    };
    categoryMessage = parentCategoryId ? 'Drafting a child category.' : 'Drafting a new root category.';
    isError = false;
  }

  async function submitCategoryForm(e: Event) {
    e.preventDefault();
    categorySaveBusy = true;
    try {
      const payload = {
        name: categoryForm.name.trim(),
        description: categoryForm.description.trim() || null,
        parentCategoryId: categoryForm.parentCategoryId ? Number(categoryForm.parentCategoryId) : null,
        status: categoryForm.status,
        displayOrder: Number(categoryForm.displayOrder),
      };

      let saved;
      if (categoryForm.id) {
        saved = await updateCategory(categoryForm.id, { ...payload, rowVersion: categoryForm.rowVersion } as CategoryUpdatePayload);
      } else {
        saved = await createCategory(payload as CategoryWritePayload);
      }

      await refreshCategories();
      focusCategory(findCategoryById(categories, saved.id) ?? saved);
      
      categoryMessage = categoryForm.id ? `Saved ${saved.name}.` : `Created ${saved.name}.`;
      isError = false;
    } catch (error) {
      categoryMessage = error instanceof Error ? error.message : 'Error saving category';
      isError = true;
    } finally {
      categorySaveBusy = false;
    }
  }

  // Helpers
  function findCategoryById(tree: CategoryTreeItem[], id: number | null): CategoryTreeItem | null {
    if (id === null) return null;
    for (const category of tree) {
      if (category.id === id) return category;
      const childMatch = findCategoryById(category.children, id);
      if (childMatch) return childMatch;
    }
    return null;
  }

  function flattenCategories(tree: CategoryTreeItem[], depth = 0): {id: number, label: string}[] {
    return tree.flatMap((cat) => [
      { id: cat.id, label: `${depth > 0 ? `${'--'.repeat(depth)} ` : ''}${cat.name}` },
      ...flattenCategories(cat.children, depth + 1),
    ]);
  }

  function categoryToForm(category: CategoryTreeItem) {
    return {
      id: category.id,
      rowVersion: category.rowVersion,
      name: category.name,
      description: category.description ?? '',
      parentCategoryId: category.parentCategoryId ? String(category.parentCategoryId) : '',
      status: category.status,
      displayOrder: String(category.displayOrder),
    };
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }
</script>

<svelte:head>
  <title>Category Atlas - Catalog Studio</title>
</svelte:head>

<div class="h-full flex flex-col xl:flex-row gap-6 animate-in fade-in duration-500">
  <!-- Left Column: Category Atlas & Pulse -->
  <div class="flex-1 flex flex-col gap-6 min-w-[50%]">
    <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h1 class="text-3xl font-bold tracking-tight text-white mb-1">Category Atlas</h1>
        <p class="text-slate-400">Shape the catalog hierarchy and structure.</p>
      </div>
      <button class="btn-primary flex items-center gap-2" onclick={() => beginCreateCategory()}>
        <Plus size={18} /> New Root Category
      </button>
    </header>

    <div class="glass-panel p-6 flex-1 flex flex-col min-h-[400px]">
      <div class="flex items-center gap-2 text-indigo-400 mb-6 font-semibold uppercase tracking-wider text-sm">
        <Layers size={18} /> Hierarchy Tree
      </div>
      
      <div class="relative flex-1 bg-slate-950/50 rounded-xl border border-slate-800/50 shadow-inner overflow-y-auto p-4 custom-scrollbar">
        {#if categoriesBusy && categories.length === 0}
          <div class="absolute inset-0 flex flex-col items-center justify-center text-slate-500 gap-3">
            <RefreshCw class="animate-spin text-indigo-500" size={32} />
            <p>Loading the atlas...</p>
          </div>
        {:else if categories.length === 0}
          <div class="flex flex-col items-center justify-center h-full text-slate-500 gap-3 pt-10">
            <Folders size={48} class="opacity-50" />
            <p>No categories define the catalog yet.</p>
          </div>
        {:else}
          <div class="space-y-1 pb-10">
            {#each categories as category (category.id)}
              <CategoryNode
                {category}
                currentId={focusedCategoryId}
                onCreateChild={beginCreateCategory}
                onSelect={focusCategory}
              />
            {/each}
          </div>
        {/if}
      </div>
    </div>
    
    <!-- Category Pulse Box (Bottom Left) -->
    {#if focusedCategory}
      <div class="glass-card p-6">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2 text-cyan-400 font-semibold uppercase tracking-wider text-sm">
            <BarChart size={18} /> Pulse of '{focusedCategory.name}'
          </div>
        </div>
        
        <div class="grid grid-cols-1 sm:grid-cols-2 text-sm gap-4 mb-4">
          <div class="bg-slate-900/50 p-3 rounded-lg border border-slate-800">
            <div class="text-slate-500 mb-1">Status</div>
            <div class="font-bold {focusedCategory.status === 'Active' ? 'text-emerald-400' : 'text-slate-400'}">{focusedCategory.status}</div>
          </div>
          <div class="bg-slate-900/50 p-3 rounded-lg border border-slate-800">
            <div class="text-slate-500 mb-1">Display Order</div>
            <div class="font-bold text-white">{focusedCategory.displayOrder}</div>
          </div>
        </div>

        {#if categoryProductsBusy}
           <div class="h-16 flex items-center justify-center"><RefreshCw class="animate-spin text-slate-500" size={20} /></div>
        {:else if categoryProducts.items.length === 0}
           <div class="bg-slate-950/50 border border-slate-800/50 rounded-xl p-4 text-center text-slate-500 text-sm">
             No immediate products found in this category layer.
           </div>
        {:else}
           <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
             {#each categoryProducts.items as prod}
               <a href="/products?id={prod.id}" class="block bg-slate-900/60 hover:bg-slate-800 border border-slate-800 rounded-lg p-3 transition-colors text-left flex justify-between items-center group">
                 <div class="truncate pr-4">
                   <h4 class="font-semibold text-slate-200 truncate group-hover:text-indigo-400 transition-colors">{prod.name}</h4>
                   <p class="text-xs text-slate-500 mt-1">{formatMoney(prod.price)} · Qty {prod.inventoryOnHand}</p>
                 </div>
                 <div class="shrink-0 text-slate-700 font-bold opacity-30 group-hover:opacity-100 group-hover:text-indigo-500 transition-colors">→</div>
               </a>
             {/each}
           </div>
        {/if}
      </div>
    {/if}
  </div>

  <!-- Right Column: Category Editor Form -->
  <div class="w-full xl:w-96 shrink-0 flex flex-col gap-6">
    <div class="glass-panel p-6 sticky top-8">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-bold text-white">
          {categoryForm.id ? 'Edit Category' : 'New Category'}
        </h2>
      </div>

      {#if categoryMessage}
        <div class="mb-4 p-3 rounded-lg text-sm border {isError ? 'bg-rose-500/10 text-rose-400 border-rose-500/20' : 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'}">
          {categoryMessage}
        </div>
      {/if}

      <form onsubmit={submitCategoryForm} class="space-y-4">
        <div>
          <label class="block text-xs font-medium text-slate-400 mb-1">Category Name</label>
          <input bind:value={categoryForm.name} required class="input-field" placeholder="E.g. Wearables" />
        </div>
        
        <div>
          <label class="block text-xs font-medium text-slate-400 mb-1">Parent Category</label>
          <select bind:value={categoryForm.parentCategoryId} class="input-field">
            <option value="">-- No Parent (Root Category) --</option>
            {#each categoryOptions.filter(o => o.id !== categoryForm.id) as option}
              <option value={option.id}>{option.label}</option>
            {/each}
          </select>
        </div>
        
        <div class="flex gap-4">
          <div class="flex-1">
            <label class="block text-xs font-medium text-slate-400 mb-1">Status</label>
            <select bind:value={categoryForm.status} class="input-field">
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-slate-400 mb-1">Display Order</label>
            <input type="number" bind:value={categoryForm.displayOrder} required class="input-field" />
          </div>
        </div>
        
        <div>
          <label class="block text-xs font-medium text-slate-400 mb-1">Description</label>
          <textarea bind:value={categoryForm.description} rows="4" class="input-field" placeholder="What belongs here?"></textarea>
        </div>
        
        <div class="pt-6 border-t border-slate-800">
          <button type="submit" disabled={categorySaveBusy} class="btn-primary w-full">
            {categorySaveBusy ? 'Saving...' : 'Save Category'}
          </button>
        </div>
      </form>
    </div>
  </div>
</div>
