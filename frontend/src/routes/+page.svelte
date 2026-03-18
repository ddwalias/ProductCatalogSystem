<script lang="ts">
  import { createQuery } from '@tanstack/svelte-query';
  import { getHealth, getCategories, getProducts, getProductAutocomplete } from '../lib/api';
  import type { CategoryTreeItem } from '../lib/types';
  import { Search, Activity, Database, Boxes } from 'lucide-svelte';
  
  import * as Card from "$lib/components/ui/card/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { Skeleton } from "$lib/components/ui/skeleton/index.js";

  const healthQuery = createQuery(() => ({
    queryKey: ['health'],
    queryFn: getHealth,
    retry: false
  }));

  const categoriesQuery = createQuery(() => ({
    queryKey: ['categories'],
    queryFn: getCategories
  }));

  const productsQuery = createQuery(() => ({
    queryKey: ['products', { pageSize: 1 }],
    queryFn: () => getProducts({ pageSize: 1 })
  }));

  let activeCategoryCount = $derived.by(() => {
    if (!categoriesQuery.data) return 0;
    const flatten = (tree: CategoryTreeItem[]): CategoryTreeItem[] => 
      tree.flatMap(c => [c, ...flatten(c.children)]);
    const allCats = flatten(categoriesQuery.data as CategoryTreeItem[]);
    return allCats.filter(c => c.status === 'Active').length;
  });

  let searchInput = $state('');
  
  const searchSuggestionsQuery = createQuery(() => ({
    queryKey: ['productAutocomplete', searchInput],
    queryFn: () => getProductAutocomplete({ query: searchInput, limit: 6 }),
    enabled: searchInput.trim().length >= 2,
  }));

  let suggestionsOpen = $derived(
    searchInput.trim().length >= 2 && 
    (searchSuggestionsQuery.isFetching || (searchSuggestionsQuery.data && searchSuggestionsQuery.data.length > 0) || searchSuggestionsQuery.isSuccess)
  );
</script>

<svelte:head>
  <title>Dashboard - Catalog Studio</title>
</svelte:head>

<div class="space-y-6">
  <div>
    <h2 class="text-3xl font-bold tracking-tight">Overview</h2>
    <p class="text-muted-foreground mt-1">Review system status and access global catalog search.</p>
  </div>

  <div class="grid gap-4 md:grid-cols-3">
    <!-- Products Card -->
    <Card.Root>
      <Card.Header class="flex flex-row items-center justify-between space-y-0 pb-2">
        <Card.Title class="text-sm font-medium">Total Products</Card.Title>
        <Boxes class="h-4 w-4 text-muted-foreground" />
      </Card.Header>
      <Card.Content>
        {#if productsQuery.isPending}
          <Skeleton class="h-8 w-24" />
        {:else if productsQuery.isError}
          <div class="text-sm text-destructive">Error</div>
        {:else}
          <div class="text-2xl font-bold">{productsQuery.data?.totalCount ?? 0}</div>
          <p class="text-xs text-muted-foreground mt-1">Across all categories</p>
        {/if}
      </Card.Content>
    </Card.Root>
    
    <!-- Categories Card -->
    <Card.Root>
      <Card.Header class="flex flex-row items-center justify-between space-y-0 pb-2">
        <Card.Title class="text-sm font-medium">Active Categories</Card.Title>
        <Database class="h-4 w-4 text-muted-foreground" />
      </Card.Header>
      <Card.Content>
        {#if categoriesQuery.isPending}
          <Skeleton class="h-8 w-24" />
        {:else if categoriesQuery.isError}
          <div class="text-sm text-destructive">Error</div>
        {:else}
          <div class="text-2xl font-bold">{activeCategoryCount}</div>
          <p class="text-xs text-muted-foreground mt-1">Hierarchically structured</p>
        {/if}
      </Card.Content>
    </Card.Root>

    <!-- Health Card -->
    <Card.Root>
      <Card.Header class="flex flex-row items-center justify-between space-y-0 pb-2">
        <Card.Title class="text-sm font-medium">API Health</Card.Title>
        <Activity class="h-4 w-4 {healthQuery.isError ? 'text-destructive' : 'text-muted-foreground'}" />
      </Card.Header>
      <Card.Content>
        {#if healthQuery.isPending}
          <Skeleton class="h-8 w-32" />
        {:else if healthQuery.isError}
          <div class="text-2xl font-bold text-destructive">Offline</div>
          <p class="text-xs text-destructive mt-1">Check database connection</p>
        {:else}
          <div class="text-2xl font-bold truncate" title={String(healthQuery.data)}>
            Online
          </div>
          <p class="text-xs text-muted-foreground mt-1 truncate">{healthQuery.data}</p>
        {/if}
      </Card.Content>
    </Card.Root>
  </div>

  <Card.Root class="overflow-visible">
    <Card.Header>
      <Card.Title>Global Search</Card.Title>
      <Card.Description>Quickly locate products deeply routed in the catalog.</Card.Description>
    </Card.Header>
    <Card.Content>
      <div class="relative max-w-xl">
        <div class="relative">
          <Search class="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input 
            bind:value={searchInput}
            type="search" 
            placeholder="Search products..." 
            class="pl-9 w-full"
          />
        </div>

        {#if suggestionsOpen}
          <div class="absolute w-full mt-2 bg-popover text-popover-foreground border rounded-md shadow-md overflow-hidden z-50 animate-in fade-in zoom-in-95">
            {#if searchSuggestionsQuery.isFetching}
              <div class="p-4 text-muted-foreground text-center text-sm">Searching...</div>
            {:else if searchSuggestionsQuery.data && searchSuggestionsQuery.data.length > 0}
              {#each searchSuggestionsQuery.data as suggestion}
                <a href="/products?search={encodeURIComponent(suggestion.name)}" class="block px-4 py-3 hover:bg-muted border-b last:border-0 transition-colors">
                  <div class="font-medium text-sm">{suggestion.name}</div>
                  <div class="text-xs text-muted-foreground">{suggestion.categoryName}</div>
                </a>
              {/each}
            {:else if searchInput.trim().length >= 2}
              <div class="p-4 text-muted-foreground text-center text-sm">No results found for "{searchInput}"</div>
            {/if}
          </div>
        {/if}
      </div>
    </Card.Content>
  </Card.Root>
</div>
