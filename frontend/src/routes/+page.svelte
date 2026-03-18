<script lang="ts">
  import { onMount } from 'svelte';
  import { getHealth, getCategories, getProducts, getProductAutocomplete } from '../lib/api';
  import type { CategoryTreeItem, ProductAutocompleteItem } from '../lib/types';
  import { Search, Activity, Database, Boxes } from 'lucide-svelte';

  let healthMessage = $state('Checking...');
  let healthTone = $state<'checking' | 'healthy' | 'degraded'>('checking');
  
  let totalProducts = $state(0);
  let activeCategoryCount = $state(0);
  
  let searchInput = $state('');
  let suggestions = $state<ProductAutocompleteItem[]>([]);
  let suggestionBusy = $state(false);
  let suggestionsOpen = $state(false);

  onMount(async () => {
    try {
      healthMessage = await getHealth();
      healthTone = 'healthy';
    } catch {
      healthMessage = 'API Offline';
      healthTone = 'degraded';
    }

    try {
      const cats = await getCategories();
      const flatten = (tree: CategoryTreeItem[]): CategoryTreeItem[] => 
        tree.flatMap(c => [c, ...flatten(c.children)]);
      const allCats = flatten(cats);
      activeCategoryCount = allCats.filter(c => c.status === 'Active').length;
    } catch {}

    try {
      const prods = await getProducts({ pageSize: 1 });
      totalProducts = prods.totalCount;
    } catch {}
  });

  $effect(() => {
    const query = searchInput.trim();
    if (query.length < 2) {
      suggestions = [];
      suggestionsOpen = false;
      return;
    }
    
    suggestionBusy = true;
    const timeout = window.setTimeout(async () => {
      try {
        suggestions = await getProductAutocomplete({ query, limit: 6 });
        suggestionsOpen = suggestions.length > 0;
      } catch {
        suggestions = [];
      } finally {
        suggestionBusy = false;
      }
    }, 200);

    return () => window.clearTimeout(timeout);
  });
</script>

<svelte:head>
  <title>Dashboard - Catalog Studio</title>
</svelte:head>

<div class="space-y-8 animate-in fade-in duration-500">
  <header>
    <h1 class="text-3xl font-bold tracking-tight text-white mb-2">Command Deck</h1>
    <p class="text-slate-400">System overview and global search</p>
  </header>

  <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
    <div class="glass-card p-6 flex items-start gap-4">
      <div class="p-3 bg-indigo-500/10 text-indigo-400 rounded-lg">
        <Boxes size={24} />
      </div>
      <div>
        <p class="text-sm text-slate-400 font-medium">Total Products</p>
        <p class="text-2xl font-bold text-white mt-1">{totalProducts}</p>
      </div>
    </div>
    
    <div class="glass-card p-6 flex items-start gap-4">
      <div class="p-3 bg-cyan-500/10 text-cyan-400 rounded-lg">
        <Database size={24} />
      </div>
      <div>
        <p class="text-sm text-slate-400 font-medium">Active Categories</p>
        <p class="text-2xl font-bold text-white mt-1">{activeCategoryCount}</p>
      </div>
    </div>

    <div class="glass-card p-6 flex items-start gap-4">
      <div class="p-3 {healthTone === 'healthy' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'} rounded-lg">
        <Activity size={24} />
      </div>
      <div>
        <p class="text-sm text-slate-400 font-medium">API Health</p>
        <p class="text-lg font-bold text-white mt-1 truncate max-w-[150px]" title={healthMessage}>{healthMessage}</p>
      </div>
    </div>
  </div>

  <div class="glass-panel p-8">
    <h2 class="text-xl font-bold text-white mb-6">Global Search</h2>
    
    <div class="relative max-w-2xl">
      <div class="relative">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
        <input 
          bind:value={searchInput}
          type="text" 
          placeholder="Search for products across the entire catalog..." 
          class="w-full bg-slate-950/50 border border-slate-700/50 rounded-xl py-4 pl-12 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all text-lg shadow-inner"
        />
      </div>

      {#if suggestionsOpen}
        <div class="absolute w-full mt-2 bg-slate-800 border border-slate-700 rounded-xl shadow-2xl overflow-hidden z-50 animate-in slide-in-from-top-2 duration-200">
          {#if suggestionBusy && suggestions.length === 0}
            <div class="p-4 text-slate-400 text-center">Searching...</div>
          {:else}
            {#each suggestions as suggestion}
              <a href="/products?search={encodeURIComponent(suggestion.name)}&id={suggestion.id}" class="block p-4 hover:bg-slate-700/50 border-b border-slate-700/50 last:border-0 transition-colors">
                <div class="font-medium text-white">{suggestion.name}</div>
                <div class="text-sm text-slate-400">{suggestion.categoryName}</div>
              </a>
            {/each}
          {/if}
        </div>
      {/if}
    </div>
  </div>
</div>
