<script lang="ts">
  import {
    createQuery,
    createMutation,
    useQueryClient,
    keepPreviousData,
  } from "@tanstack/svelte-query";
  import { page as storesPage } from "$app/stores";
  import { onMount } from "svelte";
  import {
    getProduct,
    getProducts,
    getCategories,
    createProduct,
    updateProduct,
    deleteProduct,
  } from "../../lib/api";
  import type { CategoryTreeItem, ProductDetail } from "../../lib/types";
  import {
    Plus,
    Search,
    RefreshCw,
    Trash2,
    Edit,
    PackageSearch,
    FilterX,
    Info,
    SlidersHorizontal,
  } from "lucide-svelte";

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
    queryKey: ["categories"],
    queryFn: getCategories,
  }));

  let activeCategoryOptions = $derived.by(() => {
    const cats = categoriesQuery.data || [];
    const flatten = (t: CategoryTreeItem[]): CategoryTreeItem[] =>
      t.flatMap((i) => [i, ...flatten(i.children)]);
    return flatten(cats).filter((c: CategoryTreeItem) => c.status === "Active");
  });

  let searchInput = $state("");
  let appliedSearch = $state("");
  let categoryFilter = $state("");
  let sortBy = $state("updatedAt");
  let sortDir = $state("desc");
  let pageSize = $state(10);
  let cursor = $state<string | null>(null);
  let cursorHistory = $state<(string | null)[]>([]);

  let priceFrom = $state<string>("");
  let priceTo = $state<string>("");

  let showFilters = $state(false);

  let activeFilterCount = $derived.by(() => {
    let count = 0;
    if (categoryFilter) count++;
    if (priceFrom || priceTo) count++;
    if (sortBy !== "updatedAt" || sortDir !== "desc") count++;
    return count;
  });

  const productsQuery = createQuery(() => ({
    queryKey: [
      "products",
      {
        search: appliedSearch,
        cursor,
        pageSize,
        categoryFilter,
        sortBy,
        sortDir,
        priceFrom,
        priceTo,
      },
    ],
    queryFn: () =>
      getProducts({
        query: appliedSearch || undefined,
        cursor,
        pageSize,
        categoryId: categoryFilter ? Number(categoryFilter) : null,
        priceFrom: priceFrom ? Number(priceFrom) : null,
        priceTo: priceTo ? Number(priceTo) : null,
        sortBy,
        sortDir,
      }),
    placeholderData: keepPreviousData,
  }));

  let sheetOpen = $state(false);
  let isEditing = $state(false);

  // Version viewing state
  let viewDialogOpen = $state(false);
  let isFetchingVersion = $state(false);
  let isRestoring = $state(false);
  let deleteDialogOpen = $state(false);
  let productToDelete = $state<any>(null);
  let currentMaxVersion = $state(1);
  let viewingVersionStr = $state<string>("");
  let viewingProduct = $state<any>(null);
  let viewProductDetail = $state<ProductDetail | null>(null);
  let latestRowVersionForRestore = $state<string>("");

  let productForm = $state({
    id: null as number | null,
    rowVersion: "",
    name: "",
    description: "",
    price: 0,
    inventoryOnHand: 0,
    categoryId: "",
    primaryImageUrl: "",
    inventoryReason: "",
    changedBy: "catalog-admin",
    customAttributes: [] as { key: string; value: string }[],
  });

  let originalFormSnapshot = $state<string | null>(null);

  let isFormDirty = $derived.by(() => {
    if (!originalFormSnapshot) return false;
    return JSON.stringify(productForm) !== originalFormSnapshot;
  });

  function recordToCustomAttrs(
    record: Record<string, unknown> | null,
  ): { key: string; value: string }[] {
    if (!record) return [];
    return Object.entries(record).map(([k, v]) => ({
      key: k,
      value: String(v),
    }));
  }

  function customAttrsToRecord(
    attrs: { key: string; value: string }[],
  ): Record<string, unknown> | null {
    const record: Record<string, unknown> = {};
    let hasKeys = false;
    for (const attr of attrs) {
      const k = attr.key.trim();
      const v = attr.value.trim();
      if (k) {
        record[k] = v;
        hasKeys = true;
      }
    }
    return hasKeys ? record : null;
  }

  const createProdMutation = createMutation(() => ({
    mutationFn: createProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      sheetOpen = false;
    },
  }));

  const updateProdMutation = createMutation(() => ({
    mutationFn: ({ id, payload }: { id: number; payload: any }) =>
      updateProduct(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      sheetOpen = false;
    },
  }));

  const deleteProdMutation = createMutation(() => ({
    mutationFn: deleteProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      deleteDialogOpen = false;
      productToDelete = null;
    },
  }));

  onMount(() => {
    const qs = $storesPage.url.searchParams;
    const initialSearch = qs.get("search");

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
    searchInput = "";
    appliedSearch = "";
    categoryFilter = "";
    priceFrom = "";
    priceTo = "";
    sortBy = "updatedAt";
    sortDir = "desc";
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
      rowVersion: "",
      name: "",
      description: "",
      price: 0,
      inventoryOnHand: 0,
      categoryId: activeCategoryOptions[0]
        ? String(activeCategoryOptions[0].id)
        : "",
      primaryImageUrl: "",
      inventoryReason: "",
      changedBy: "catalog-admin",
      customAttributes: [],
    };
    originalFormSnapshot = JSON.stringify(productForm);
    sheetOpen = true;
  }

  async function editProduct(product: any) {
    try {
      const detail = await getProduct(product.id, product.versionNumber);
      isEditing = true;
      productForm = {
        id: detail.id,
        rowVersion: detail.rowVersion,
        name: detail.name,
        description: detail.description ?? "",
        price: detail.price,
        inventoryOnHand: detail.inventoryOnHand,
        categoryId: String(detail.categoryId),
        primaryImageUrl: detail.primaryImageUrl ?? "",
        inventoryReason: "",
        changedBy: "catalog-admin",
        customAttributes: recordToCustomAttrs(detail.customAttributes),
      };
      originalFormSnapshot = JSON.stringify(productForm);
      sheetOpen = true;
    } catch (err) {
      console.error("Failed to load product details for editing", err);
    }
  }

  async function openViewDialog(product: any) {
    viewingProduct = product;
    currentMaxVersion = product.versionNumber || 1;
    viewingVersionStr = String(currentMaxVersion);
    await loadViewVersion(product.id, currentMaxVersion);
    if (viewProductDetail) {
      latestRowVersionForRestore = viewProductDetail.rowVersion;
    }
    viewDialogOpen = true;
  }

  async function loadViewVersion(id: number, version: number) {
    isFetchingVersion = true;
    try {
      viewProductDetail = await getProduct(id, version);
    } catch (err) {
      console.error("Failed to fetch product version", err);
    } finally {
      isFetchingVersion = false;
    }
  }

  function handleViewVersionChange(newVal: string) {
    if (!newVal || !viewingProduct) return;
    viewingVersionStr = newVal;
    loadViewVersion(viewingProduct.id, Number(newVal));
  }

  async function restoreSelectedVersion() {
    if (!viewingProduct || !viewProductDetail) return;
    if (
      !confirm(
        `Are you sure you want to revert ${viewProductDetail.name} to version ${viewingVersionStr}?`,
      )
    )
      return;

    isRestoring = true;
    try {
      const payload = {
        name: viewProductDetail.name,
        description: viewProductDetail.description,
        price: viewProductDetail.price,
        inventoryOnHand: viewProductDetail.inventoryOnHand,
        categoryId: viewProductDetail.categoryId,
        primaryImageUrl: viewProductDetail.primaryImageUrl,
        customAttributes: viewProductDetail.customAttributes,
        inventoryReason: `Restored to version ${viewingVersionStr}`,
        changedBy: "catalog-admin",
      };

      updateProdMutation.mutate({
        id: viewingProduct.id,
        payload: { ...payload, rowVersion: latestRowVersionForRestore },
      });
      // viewDialogOpen = false; -> we'll let onSuccess handle closing, or user closing if error
    } catch (err) {
      console.error("Failed to restore version", err);
    } finally {
      isRestoring = false;
    }
  }

  function submitProductForm(e: Event) {
    e.preventDefault();

    const currentParams = {
      name: productForm.name.trim(),
      description: productForm.description.trim() || null,
      price: Number(productForm.price),
      inventoryOnHand: Number(productForm.inventoryOnHand),
      categoryId: Number(productForm.categoryId),
      primaryImageUrl: productForm.primaryImageUrl.trim() || null,
      customAttributes: customAttrsToRecord(productForm.customAttributes),
      inventoryReason: productForm.inventoryReason.trim() || null,
      changedBy: productForm.changedBy.trim() || null,
    };

    if (productForm.id) {
      if (!isFormDirty) return;

      const diffPayload: Record<string, any> = {
        rowVersion: productForm.rowVersion,
      };
      const originalForm = JSON.parse(originalFormSnapshot!);

      const originalParams = {
        name: originalForm.name.trim(),
        description: originalForm.description.trim() || null,
        price: Number(originalForm.price),
        inventoryOnHand: Number(originalForm.inventoryOnHand),
        categoryId: Number(originalForm.categoryId),
        primaryImageUrl: originalForm.primaryImageUrl.trim() || null,
        customAttributes: customAttrsToRecord(originalForm.customAttributes),
        inventoryReason: originalForm.inventoryReason.trim() || null,
        changedBy: originalForm.changedBy.trim() || null,
      };

      for (const [key, val] of Object.entries(currentParams)) {
        if (
          JSON.stringify(val) !==
          JSON.stringify(originalParams[key as keyof typeof originalParams])
        ) {
          diffPayload[key] = val;
        }
      }

      updateProdMutation.mutate({ id: productForm.id, payload: diffPayload });
    } else {
      createProdMutation.mutate(currentParams);
    }
  }

  function openDeleteDialog(product: any) {
    productToDelete = product;
    deleteDialogOpen = true;
  }

  function executeDelete() {
    if (!productToDelete) return;
    deleteProdMutation.mutate(productToDelete.id);
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(value);
  }
</script>

<svelte:head>
  <title>Products - Catalog Studio</title>
</svelte:head>

<div class="space-y-6">
  <div
    class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
  >
    <div>
      <h2 class="text-3xl font-bold tracking-tight">Products</h2>
      <p class="text-muted-foreground mt-1">
        Manage your catalog inventory and supply.
      </p>
    </div>
    <Button onclick={beginDrafting}>
      <Plus class="mr-2 h-4 w-4" /> Add Product
    </Button>
  </div>

  <Card.Root>
    <!-- Filter Bar -->
    <div class="p-4 border-b bg-muted/20 flex flex-col gap-4">
      <div class="flex items-center gap-3 w-full">
        <div class="relative w-full flex-1 max-w-sm">
          <Search
            class="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground"
          />
          <Input
            bind:value={searchInput}
            onkeydown={(e) => {
              if (e.key === "Enter") applySearchFilter();
            }}
            placeholder="Search products..."
            class="pl-9 h-9 w-full bg-background"
          />
        </div>
        <Button
          variant={showFilters || activeFilterCount > 0
            ? "secondary"
            : "outline"}
          size="sm"
          class="h-9 gap-2 shrink-0"
          onclick={() => (showFilters = !showFilters)}
        >
          <SlidersHorizontal class="h-4 w-4" />
          <span class="hidden sm:inline">Filters</span>
          {#if activeFilterCount > 0}
            <Badge
              variant="secondary"
              class="ml-1 rounded-sm px-1.5 h-5 font-normal bg-background/50 border"
              >{activeFilterCount}</Badge
            >
          {/if}
        </Button>
      </div>

      {#if showFilters}
        <div
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 pt-4 border-t border-border/60"
        >
          <!-- Category Filter -->
          <div class="space-y-2">
            <Label class="text-xs text-muted-foreground">Category</Label>
            <Select.Root
              type="single"
              bind:value={categoryFilter}
              onValueChange={() => resetCursor()}
            >
              <Select.Trigger class="w-full h-9 bg-background">
                {categoryFilter
                  ? (activeCategoryOptions.find(
                      (c) => String(c.id) === categoryFilter,
                    )?.name ?? "All Categories")
                  : "All Categories"}
              </Select.Trigger>
              <Select.Content>
                <Select.Item value="">All Categories</Select.Item>
                {#each activeCategoryOptions as cat}
                  <Select.Item value={String(cat.id)}>{cat.name}</Select.Item>
                {/each}
              </Select.Content>
            </Select.Root>
          </div>

          <!-- Price Range Filter -->
          <div class="space-y-2">
            <Label class="text-xs text-muted-foreground">Price Range</Label>
            <div class="flex gap-2 items-center">
              <Input
                type="number"
                step="0.01"
                placeholder="Min $"
                class="w-full h-9 bg-background"
                bind:value={priceFrom}
              />
              <span class="text-muted-foreground/60">-</span>
              <Input
                type="number"
                step="0.01"
                placeholder="Max $"
                class="w-full h-9 bg-background"
                bind:value={priceTo}
              />
            </div>
          </div>

          <!-- Sorting Filters -->
          <div class="space-y-2">
            <Label class="text-xs text-muted-foreground">Sort and Order</Label>
            <div class="flex gap-2 items-center">
              <Select.Root
                type="single"
                bind:value={sortBy}
                onValueChange={() => resetCursor()}
              >
                <Select.Trigger class="w-full h-9 bg-background">
                  {sortBy === "updatedAt"
                    ? "Latest"
                    : sortBy === "name"
                      ? "Name"
                      : sortBy === "price"
                        ? "Price"
                        : "Inventory"}
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
                <Select.Trigger class="w-[85px] shrink-0 h-9 bg-background">
                  {sortDir === "desc" ? "Desc" : "Asc"}
                </Select.Trigger>
                <Select.Content>
                  <Select.Item value="desc">Desc</Select.Item>
                  <Select.Item value="asc">Asc</Select.Item>
                </Select.Content>
              </Select.Root>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-end pb-[2px]">
            {#if appliedSearch || categoryFilter || priceFrom || priceTo || sortBy !== "updatedAt" || sortDir !== "desc"}
              <Button
                variant="ghost"
                size="sm"
                onclick={resetCatalogFilters}
                class="h-9 px-3 text-muted-foreground hover:text-foreground"
              >
                <FilterX class="h-4 w-4 mr-2" /> Clear all
              </Button>
            {/if}
          </div>
        </div>
      {/if}
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
          {#if !productsQuery.data}
            <Table.Row>
              <Table.Cell colspan={6} class="h-32 text-center">
                <RefreshCw
                  class="animate-spin text-muted-foreground mx-auto h-6 w-6"
                />
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
              <Table.Cell
                colspan={6}
                class="h-48 text-center text-muted-foreground"
              >
                <PackageSearch
                  class="mx-auto h-8 w-8 mb-3 text-muted-foreground/50"
                />
                No products found. Allow filters to be less restrictive.
              </Table.Cell>
            </Table.Row>
          {:else}
            {#each productsQuery.data?.items || [] as product (product.id)}
              <Table.Row class="hover:bg-muted/50 transition-colors">
                <Table.Cell>
                  <div
                    class="h-10 w-10 flex items-center justify-center rounded-md border bg-muted overflow-hidden"
                  >
                    {#if product.primaryImageUrl}
                      <img
                        src={product.primaryImageUrl}
                        alt={product.name}
                        class="h-full w-full object-cover"
                      />
                    {:else}
                      <span
                        class="text-muted-foreground font-semibold uppercase text-xs"
                        >{product.name.charAt(0)}</span
                      >
                    {/if}
                  </div>
                </Table.Cell>
                <Table.Cell>
                  <div class="font-medium text-foreground">{product.name}</div>
                  <div class="text-xs text-muted-foreground mt-0.5">
                    {product.categoryName}
                  </div>
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
                  <span
                    class={product.inventoryOnHand > 10
                      ? "text-foreground"
                      : product.inventoryOnHand > 0
                        ? "text-orange-500 font-medium"
                        : "text-destructive font-medium"}
                  >
                    {product.inventoryOnHand}
                  </span>
                </Table.Cell>
                <Table.Cell>
                  <Button
                    variant="ghost"
                    size="icon"
                    onclick={() => openViewDialog(product)}
                  >
                    <Info class="h-4 w-4 text-muted-foreground" />
                    <span class="sr-only">View Details</span>
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    onclick={() => editProduct(product)}
                  >
                    <Edit class="h-4 w-4 text-muted-foreground" />
                    <span class="sr-only">Edit</span>
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    class="text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                    onclick={() => openDeleteDialog(product)}
                  >
                    <Trash2 class="h-4 w-4" />
                    <span class="sr-only">Delete</span>
                  </Button>
                </Table.Cell>
              </Table.Row>
            {/each}
          {/if}
        </Table.Body>
      </Table.Root>
    </div>

    <div
      class="p-4 border-t flex flex-col md:flex-row items-center justify-between gap-4 text-sm bg-muted/10"
    >
      <div class="text-muted-foreground">
        Showing <span class="font-medium text-foreground"
          >{productsQuery.data?.items.length ?? 0}</span
        >
        of {productsQuery.data?.totalCount ?? 0} total.
      </div>
      <div class="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          class="h-8"
          disabled={cursorHistory.length === 0 || productsQuery.isFetching}
          onclick={goToPreviousPage}>Previous</Button
        >
        <Button
          variant="outline"
          size="sm"
          class="h-8"
          disabled={!productsQuery.data?.nextCursor || productsQuery.isFetching}
          onclick={goToNextPage}>Next</Button
        >
      </div>
    </div>
  </Card.Root>
</div>

<!-- Product Editor Dialog -->
<Dialog.Root bind:open={sheetOpen}>
  <Dialog.Content class="w-full sm:max-w-md max-h-[90vh] overflow-y-auto">
    <Dialog.Header>
      <Dialog.Title
        >{productForm.id ? "Edit Product" : "Create Product"}</Dialog.Title
      >
      <Dialog.Description>
        Fill in the details for this item.
      </Dialog.Description>
    </Dialog.Header>

    <form onsubmit={submitProductForm} class="space-y-6 py-6">
      <div class="space-y-4">
        <div class="space-y-2">
          <Label for="name">Name</Label>
          <Input
            id="name"
            bind:value={productForm.name}
            required
            placeholder="Product Title"
          />
        </div>

        <div class="space-y-2">
          <Label for="category">Category</Label>
          <Select.Root type="single" bind:value={productForm.categoryId}>
            <Select.Trigger class="w-full">
              {productForm.categoryId
                ? (activeCategoryOptions.find(
                    (c) => String(c.id) === productForm.categoryId,
                  )?.name ?? "Select Category")
                : "Select Category"}
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
            <Input
              id="price"
              type="number"
              step="0.01"
              bind:value={productForm.price}
              required
            />
          </div>
          <div class="space-y-2">
            <Label for="stock">Inventory Stock</Label>
            <Input
              id="stock"
              type="number"
              bind:value={productForm.inventoryOnHand}
              required
            />
          </div>
        </div>

        <div class="space-y-2">
          <Label for="imageUrl">Image URL</Label>
          <Input
            id="imageUrl"
            bind:value={productForm.primaryImageUrl}
            placeholder="https://..."
          />
        </div>

        <div class="space-y-2">
          <Label for="description">Description</Label>
          <Textarea
            id="description"
            bind:value={productForm.description}
            rows={3}
            placeholder="Detailed product description..."
            class="resize-none"
          />
        </div>

        <div class="space-y-2">
          <Label>Custom Attributes</Label>
          <div class="space-y-2">
            {#each productForm.customAttributes as attr, i}
              <div class="flex gap-2 items-center">
                <Input
                  bind:value={productForm.customAttributes[i].key}
                  placeholder="Key (e.g. Color)"
                  class="flex-1 h-9 bg-background"
                />
                <Input
                  bind:value={productForm.customAttributes[i].value}
                  placeholder="Value (e.g. Red)"
                  class="flex-1 h-9 bg-background"
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  class="h-9 w-9 text-muted-foreground hover:text-destructive shrink-0"
                  onclick={() =>
                    (productForm.customAttributes =
                      productForm.customAttributes.filter(
                        (_, idx) => idx !== i,
                      ))}
                >
                  <Trash2 class="h-4 w-4" />
                </Button>
              </div>
            {/each}
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            class="w-full"
            onclick={() =>
              (productForm.customAttributes = [
                ...productForm.customAttributes,
                { key: "", value: "" },
              ])}
          >
            <Plus class="h-4 w-4 mr-2" /> Add Attribute
          </Button>
        </div>
      </div>

      <Dialog.Footer
        class="flex flex-col-reverse sm:flex-row sm:justify-end items-center w-full gap-2 mt-4"
      >
        <div class="flex flex-col gap-2 w-full sm:w-auto items-end">
          <div class="flex gap-2">
            <Button
              type="button"
              variant="ghost"
              onclick={() => (sheetOpen = false)}>Cancel</Button
            >
            <Button
              type="submit"
              disabled={!isFormDirty ||
                createProdMutation.isPending ||
                updateProdMutation.isPending}
            >
              {createProdMutation.isPending || updateProdMutation.isPending
                ? "Saving..."
                : "Save Product"}
            </Button>
          </div>
          {#if updateProdMutation.isError}
            <span
              class="text-xs text-destructive bg-destructive/10 p-1.5 px-2 rounded-md font-medium border border-destructive/20"
              >{updateProdMutation.error?.message ||
                "Version conflict or server error. Refresh and try again."}</span
            >
          {/if}
        </div>
      </Dialog.Footer>
    </form>
  </Dialog.Content>
</Dialog.Root>

<!-- Product History / View Dialog -->
<Dialog.Root bind:open={viewDialogOpen}>
  <Dialog.Content class="w-full sm:max-w-md max-h-[90vh] overflow-y-auto">
    <Dialog.Header>
      <Dialog.Title>Product History</Dialog.Title>
      <Dialog.Description>
        Review historical data iterations.
      </Dialog.Description>
    </Dialog.Header>

    {#if currentMaxVersion > 1}
      <div
        class="flex items-center gap-4 bg-muted/40 p-3 rounded-md mt-4 border border-border/50"
      >
        <div class="flex items-center gap-3 w-full">
          <Label
            class="text-xs uppercase tracking-wider text-muted-foreground whitespace-nowrap"
            >View</Label
          >
          <Select.Root
            type="single"
            value={viewingVersionStr}
            onValueChange={(v) => handleViewVersionChange(v)}
          >
            <Select.Trigger
              class="w-full sm:w-[150px] h-8 text-xs bg-background"
            >
              {viewingVersionStr ? `Version ${viewingVersionStr}` : "Select"}
            </Select.Trigger>
            <Select.Content>
              {#each Array(currentMaxVersion)
                .fill(0)
                .map((_, i) => currentMaxVersion - i) as v}
                <Select.Item value={String(v)}>
                  v{v}
                  {v === currentMaxVersion ? "(Latest)" : ""}
                </Select.Item>
              {/each}
            </Select.Content>
          </Select.Root>
        </div>
        {#if viewingVersionStr !== String(currentMaxVersion)}
          <Badge
            variant="secondary"
            class="bg-amber-500/10 text-amber-600 border-amber-500/20 whitespace-nowrap shrink-0"
            >Historical</Badge
          >
        {/if}
      </div>
    {/if}

    <div
      class="space-y-6 pt-4 {isFetchingVersion
        ? 'opacity-50 pointer-events-none'
        : 'transition-opacity'}"
    >
      <div class="space-y-4">
        <div class="space-y-1">
          <Label class="text-xs text-muted-foreground">Name</Label>
          <p class="font-medium text-sm p-3 bg-muted/20 border rounded-md">
            {viewProductDetail?.name ?? "..."}
          </p>
        </div>

        <div class="space-y-1">
          <Label class="text-xs text-muted-foreground">Category</Label>
          <p class="font-medium text-sm p-3 bg-muted/20 border rounded-md">
            {viewProductDetail?.categoryName ?? "..."}
          </p>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-1">
            <Label class="text-xs text-muted-foreground">Price</Label>
            <p class="font-medium text-sm p-3 bg-muted/20 border rounded-md">
              {viewProductDetail ? formatMoney(viewProductDetail.price) : "..."}
            </p>
          </div>
          <div class="space-y-1">
            <Label class="text-xs text-muted-foreground">Inventory</Label>
            <p class="font-medium text-sm p-3 bg-muted/20 border rounded-md">
              {viewProductDetail?.inventoryOnHand ?? "..."}
            </p>
          </div>
        </div>

        <div class="space-y-1">
          <Label class="text-xs text-muted-foreground">Description</Label>
          <div
            class="text-sm p-3 bg-muted/20 border rounded-md min-h-[4rem] text-muted-foreground"
          >
            {viewProductDetail?.description || "No description provided."}
          </div>
        </div>

        <div class="space-y-1">
          <Label class="text-xs text-muted-foreground">Custom Attributes</Label>
          {#if viewProductDetail?.customAttributes && Object.keys(viewProductDetail.customAttributes).length > 0}
            <div class="border rounded-md divide-y overflow-hidden">
              {#each Object.entries(viewProductDetail.customAttributes) as [key, value]}
                <div class="grid grid-cols-2 divide-x bg-background">
                  <div
                    class="p-2.5 text-xs font-medium text-muted-foreground bg-muted/20 truncate"
                    title={key}
                  >
                    {key}
                  </div>
                  <div
                    class="p-2.5 text-xs text-foreground truncate"
                    title={String(value)}
                  >
                    {String(value)}
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <div
              class="text-sm p-3 bg-muted/20 border rounded-md min-h-[2.5rem] text-muted-foreground"
            >
              None
            </div>
          {/if}
        </div>
      </div>

      <Dialog.Footer class="flex flex-col gap-2 w-full pt-2">
        <div class="flex gap-2 w-full justify-end">
          <Button variant="ghost" onclick={() => (viewDialogOpen = false)}
            >Close</Button
          >

          {#if viewingVersionStr !== String(currentMaxVersion)}
            <Button
              variant="default"
              disabled={isFetchingVersion ||
                isRestoring ||
                updateProdMutation.isPending}
              onclick={restoreSelectedVersion}
            >
              {isRestoring || updateProdMutation.isPending
                ? "Restoring..."
                : "Restore as Latest"}
            </Button>
          {/if}
        </div>
        {#if updateProdMutation.isError}
          <div class="w-full flex justify-end">
            <span
              class="text-xs text-destructive bg-destructive/10 p-1.5 px-2 rounded-md font-medium border border-destructive/20"
              >{updateProdMutation.error?.message ||
                "Version conflict or server error. Refresh and try again."}</span
            >
          </div>
        {/if}
      </Dialog.Footer>
    </div>
  </Dialog.Content>
</Dialog.Root>

<!-- Dedicated Delete Confirmation Dialog -->
<Dialog.Root bind:open={deleteDialogOpen}>
  <Dialog.Content class="w-full sm:max-w-md">
    <Dialog.Header>
      <Dialog.Title>Delete Product</Dialog.Title>
      <Dialog.Description>
        Are you sure you want to permanently delete <strong
          class="text-foreground">{productToDelete?.name}</strong
        >? This action cannot be undone.
      </Dialog.Description>
    </Dialog.Header>
    <Dialog.Footer class="flex gap-2 justify-end mt-4">
      <Button variant="ghost" onclick={() => (deleteDialogOpen = false)}
        >Cancel</Button
      >
      <Button
        variant="destructive"
        disabled={deleteProdMutation.isPending}
        onclick={executeDelete}
      >
        {deleteProdMutation.isPending ? "Deleting..." : "Delete"}
      </Button>
    </Dialog.Footer>
  </Dialog.Content>
</Dialog.Root>
