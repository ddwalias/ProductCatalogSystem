<script lang="ts">
  import { createQuery, createMutation, useQueryClient } from '@tanstack/svelte-query';
  import { page as storesPage } from '$app/stores';
  import { onMount } from 'svelte';
  import { getProducts, getCategories, createProduct, updateProduct, deleteProduct } from '../../lib/api';
  import type { CategoryTreeItem, ProductDetail } from '../../lib/types';
  import { Plus, Search, RefreshCw, Trash2, Edit, PackageSearch, FilterX } from 'lucide-svelte';

  // Shadcn Components
  import { Button } from "$lib/components/ui/button/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { Label } from "$lib/components/ui/label/index.js";
  import { Textarea } from "$lib/components/ui/textarea/index.js";
  import { Badge } from "$lib/components/ui/badge/index.js";
  import * as Table from "$lib/components/ui/table/index.js";
  import * as Card from "$lib/components/ui/card/index.js";
  import * as Dialog from "$lib/components/ui/dialog/index.js";
  import * as Select from "$lib/components/ui/select/index.js";

  const queryClient = useQueryClient();

  const categoriesQuery = createQuery(() => ({
    queryKey: ['categories'],
    queryFn: getCategories
  }));

  let activeCategoryOptions = $derived.by(() => {
    const cats = categoriesQuery.data || [];
    const flatten = (t: CategoryTreeItem[]): CategoryTreeItem[] => t.flatMap(i => [i, ...flatten(i.children)]);
    return flatten(cats).filter((c: CategoryTreeItem) => c.status === 'Active');
  });

  let searchInput = $state('');
  let appliedSearch = $state('');
  let categoryFilter = $state('');
  let sortBy = $state('updatedAt');
  let sortDir = $state('desc');
  let pageSize = $state(10);
  let cursor = $state<string | null>(null);
  let cursorHistory = $state<(string | null)[]>([]);

  const productsQuery = createQuery(() => ({
    queryKey: ['products', { search: appliedSearch, cursor, pageSize, categoryFilter, sortBy, sortDir }],
    queryFn: () => getProducts({
      query: appliedSearch || undefined,
      cursor,
      pageSize,
      categoryId: categoryFilter ? Number(categoryFilter) : null,
      sortBy,
      sortDir,
    }),
    placeholderData: (prev) => prev
  }));

  let sheetOpen = $state(false);
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

  const createProdMutation = createMutation(() => ({
    mutationFn: createProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      sheetOpen = false;
    }
  }));

  const updateProdMutation = createMutation(() => ({
    mutationFn: ({ id, payload }: { id: number, payload: any }) => updateProduct(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      sheetOpen = false;
    }
  }));

  const deleteProdMutation = createMutation(() => ({
    mutationFn: deleteProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      sheetOpen = false;
    }
  }));

  onMount(() => {
    const qs = $storesPage.url.searchParams;
    const initialSearch = qs.get('search');
    
    if (initialSearch) {
      searchInput = initialSearch;
      appliedSearch = initialSearch;
    }
  });

  function applySearchFilter() {
    resetCursor();
    appliedSearch = searchInput.trim();
  }

  function resetCatalogFilters() {
    searchInput = '';
    appliedSearch = '';
    categoryFilter = '';
    sortBy = 'updatedAt';
    sortDir = 'desc';
    pageSize = 10;
    resetCursor();
  }

  function resetCursor() {
    cursor = null;
    cursorHistory = [];
  }

  function goToNextPage() {
    if (!productsQuery.data?.nextCursor) return;
    cursorHistory = [...cursorHistory, cursor];
    cursor = productsQuery.data.nextCursor;
  }

  function goToPreviousPage() {
    if (cursorHistory.length === 0) return;
    const nextHistory = [...cursorHistory];
    const previousCursor = nextHistory.pop() ?? null;
    cursorHistory = nextHistory;
    cursor = previousCursor;
  }

  function beginDrafting() {
    isEditing = false;
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
    sheetOpen = true;
  }

  function editProduct(product: any) {
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
    sheetOpen = true;
  }

  function submitProductForm(e: Event) {
    e.preventDefault();
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

    if (productForm.id) {
      updateProdMutation.mutate({ id: productForm.id, payload: { ...payload, rowVersion: productForm.rowVersion } });
    } else {
      createProdMutation.mutate(payload);
    }
  }

  function deleteSelected() {
    if (!productForm.id) return;
    if (!confirm(`Are you sure you want to delete ${productForm.name}?`)) return;
    deleteProdMutation.mutate(productForm.id);
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }
</script>

<svelte:head>
  <title>Products - Catalog Studio</title>
</svelte:head>

<div class="space-y-6">
  <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
    <div>
      <h2 class="text-3xl font-bold tracking-tight">Products</h2>
      <p class="text-muted-foreground mt-1">Manage your catalog inventory and supply.</p>
    </div>
    <Button onclick={beginDrafting}>
      <Plus class="mr-2 h-4 w-4" /> Add Product
    </Button>
  </div>

  <Card.Root>
    <!-- Filter Bar -->
    <div class="p-4 border-b flex flex-col md:flex-row gap-4 items-center bg-muted/20">
      <div class="relative w-full flex-1 max-w-sm">
        <Search class="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input 
          bind:value={searchInput}
          onkeydown={(e) => { if (e.key === 'Enter') applySearchFilter(); }}
          placeholder="Search products..."
          class="pl-9 h-9 w-full bg-background"
        />
      </div>
      
      <div class="flex flex-wrap gap-3 w-full md:w-auto">
        <Select.Root 
          type="single"
          bind:value={categoryFilter}
          onValueChange={() => resetCursor()}
        >
          <Select.Trigger class="w-[180px] bg-background">
            {categoryFilter ? (activeCategoryOptions.find(c => String(c.id) === categoryFilter)?.name ?? 'All Categories') : 'All Categories'}
          </Select.Trigger>
          <Select.Content>
            <Select.Item value="">All Categories</Select.Item>
            {#each activeCategoryOptions as cat}
              <Select.Item value={String(cat.id)}>{cat.name}</Select.Item>
            {/each}
          </Select.Content>
        </Select.Root>

        <Select.Root 
          type="single"
          bind:value={sortBy}
          onValueChange={() => resetCursor()}
        >
          <Select.Trigger class="w-[120px] bg-background">
            {sortBy === 'updatedAt' ? 'Latest' : sortBy === 'name' ? 'Name' : sortBy === 'price' ? 'Price' : 'Inventory'}
          </Select.Trigger>
          <Select.Content>
            <Select.Item value="updatedAt">Latest</Select.Item>
            <Select.Item value="name">Name</Select.Item>
            <Select.Item value="price">Price</Select.Item>
            <Select.Item value="inventory">Inventory</Select.Item>
          </Select.Content>
        </Select.Root>

        <Select.Root 
          type="single"
          bind:value={sortDir}
          onValueChange={() => resetCursor()}
        >
          <Select.Trigger class="w-[100px] bg-background">
            {sortDir === 'desc' ? 'Desc' : 'Asc'}
          </Select.Trigger>
          <Select.Content>
            <Select.Item value="desc">Desc</Select.Item>
            <Select.Item value="asc">Asc</Select.Item>
          </Select.Content>
        </Select.Root>
        
        {#if appliedSearch || categoryFilter || sortBy !== 'updatedAt'}
          <Button variant="ghost" size="sm" onclick={resetCatalogFilters} class="h-9 px-2 text-muted-foreground hover:text-foreground">
            <FilterX class="h-4 w-4 mr-1" /> Clear
          </Button>
        {/if}
      </div>
    </div>

    <div class="w-full overflow-auto">
      <Table.Root>
        <Table.Header>
          <Table.Row>
            <Table.Head class="w-[60px]"></Table.Head>
            <Table.Head>Product</Table.Head>
            <Table.Head>Status</Table.Head>
            <Table.Head class="text-right">Price</Table.Head>
            <Table.Head class="text-right">Stock</Table.Head>
            <Table.Head class="w-[60px]"></Table.Head>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {#if productsQuery.isFetching && !productsQuery.isPlaceholderData}
            <Table.Row>
              <Table.Cell colspan={6} class="h-32 text-center">
                <RefreshCw class="animate-spin text-muted-foreground mx-auto h-6 w-6" />
              </Table.Cell>
            </Table.Row>
          {:else if productsQuery.isError}
            <Table.Row>
              <Table.Cell colspan={6} class="h-32 text-center text-destructive">
                Failed to load products. {String(productsQuery.error)}
              </Table.Cell>
            </Table.Row>
          {:else if productsQuery.data?.items.length === 0}
            <Table.Row>
              <Table.Cell colspan={6} class="h-48 text-center text-muted-foreground">
                <PackageSearch class="mx-auto h-8 w-8 mb-3 text-muted-foreground/50" />
                No products found. Allow filters to be less restrictive.
              </Table.Cell>
            </Table.Row>
          {:else}
            {#each productsQuery.data?.items || [] as product}
              <Table.Row class="hover:bg-muted/50 transition-colors">
                <Table.Cell>
                  <div class="h-10 w-10 flex items-center justify-center rounded-md border bg-muted overflow-hidden">
                    {#if product.primaryImageUrl}
                      <img src={product.primaryImageUrl} alt={product.name} class="h-full w-full object-cover" />
                    {:else}
                      <span class="text-muted-foreground font-semibold uppercase text-xs">{product.name.charAt(0)}</span>
                    {/if}
                  </div>
                </Table.Cell>
                <Table.Cell>
                  <div class="font-medium text-foreground">{product.name}</div>
                  <div class="text-xs text-muted-foreground mt-0.5">{product.categoryName}</div>
                </Table.Cell>
                <Table.Cell>
                  <Badge variant="outline" class="font-normal text-xs">
                    v{product.versionNumber}
                  </Badge>
                </Table.Cell>
                <Table.Cell class="text-right font-medium">
                  {formatMoney(product.price)}
                </Table.Cell>
                <Table.Cell class="text-right">
                  <span class={product.inventoryOnHand > 10 ? 'text-foreground' : product.inventoryOnHand > 0 ? 'text-orange-500 font-medium' : 'text-destructive font-medium'}>
                    {product.inventoryOnHand}
                  </span>
                </Table.Cell>
                <Table.Cell>
                  <Button variant="ghost" size="icon" onclick={() => editProduct(product)}>
                    <Edit class="h-4 w-4 text-muted-foreground" />
                    <span class="sr-only">Edit</span>
                  </Button>
                </Table.Cell>
              </Table.Row>
            {/each}
          {/if}
        </Table.Body>
      </Table.Root>
    </div>
    
    <div class="p-4 border-t flex flex-col md:flex-row items-center justify-between gap-4 text-sm bg-muted/10">
      <div class="text-muted-foreground">
        Showing <span class="font-medium text-foreground">{productsQuery.data?.items.length ?? 0}</span> of {productsQuery.data?.totalCount ?? 0} total.
      </div>
      <div class="flex items-center gap-2">
        <Button variant="outline" size="sm" class="h-8" disabled={cursorHistory.length === 0} onclick={goToPreviousPage}>Previous</Button>
        <Button variant="outline" size="sm" class="h-8" disabled={!productsQuery.data?.nextCursor} onclick={goToNextPage}>Next</Button>
      </div>
    </div>
  </Card.Root>
</div>

<!-- Product Editor Dialog -->
<Dialog.Root bind:open={sheetOpen}>
  <Dialog.Content class="w-full sm:max-w-md max-h-[90vh] overflow-y-auto">
    <Dialog.Header>
      <Dialog.Title>{productForm.id ? 'Edit Product' : 'Create Product'}</Dialog.Title>
      <Dialog.Description>
        Fill in the details for this item.
      </Dialog.Description>
    </Dialog.Header>

    <form onsubmit={submitProductForm} class="space-y-6 py-6">
      <div class="space-y-4">
        <div class="space-y-2">
          <Label for="name">Name</Label>
          <Input id="name" bind:value={productForm.name} required placeholder="Product Title" />
        </div>
        
        <div class="space-y-2">
          <Label for="category">Category</Label>
          <Select.Root
            type="single"
            bind:value={productForm.categoryId}
          >
            <Select.Trigger class="w-full">
              {productForm.categoryId ? (activeCategoryOptions.find(c => String(c.id) === productForm.categoryId)?.name ?? 'Select Category') : 'Select Category'}
            </Select.Trigger>
            <Select.Content>
              {#each activeCategoryOptions as cat}
                <Select.Item value={String(cat.id)}>{cat.name}</Select.Item>
              {/each}
            </Select.Content>
          </Select.Root>
        </div>
        
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label for="price">Price ($)</Label>
            <Input id="price" type="number" step="0.01" bind:value={productForm.price} required />
          </div>
          <div class="space-y-2">
            <Label for="stock">Inventory Stock</Label>
            <Input id="stock" type="number" bind:value={productForm.inventoryOnHand} required />
          </div>
        </div>

        <div class="space-y-2">
          <Label for="imageUrl">Image URL</Label>
          <Input id="imageUrl" bind:value={productForm.primaryImageUrl} placeholder="https://..." />
        </div>
        
        <div class="space-y-2">
          <Label for="description">Description</Label>
          <Textarea id="description" bind:value={productForm.description} rows={4} placeholder="Detailed product description..." class="resize-none" />
        </div>
      </div>

      <Dialog.Footer class="flex flex-col-reverse sm:flex-row sm:justify-between items-center w-full gap-2 mt-4">
        <div class="w-full sm:w-auto">
          {#if productForm.id}
            <Button type="button" variant="destructive" class="w-full sm:w-auto" onclick={deleteSelected} disabled={deleteProdMutation.isPending}>
              <Trash2 class="mr-2 h-4 w-4" /> Delete
            </Button>
          {/if}
        </div>
        <div class="flex gap-2 w-full sm:w-auto justify-end">
          <Button type="button" variant="ghost" onclick={() => sheetOpen = false}>Cancel</Button>
          <Button type="submit" disabled={createProdMutation.isPending || updateProdMutation.isPending}>
            {createProdMutation.isPending || updateProdMutation.isPending ? 'Saving...' : 'Save Product'}
          </Button>
        </div>
      </Dialog.Footer>
    </form>
  </Dialog.Content>
</Dialog.Root>
