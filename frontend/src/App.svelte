<script lang="ts">
  import { onMount } from 'svelte';
  import {
    ApiError,
    createCategory,
    createProduct,
    deleteProduct,
    getCategories,
    getCategoryProducts,
    getHealth,
    getProduct,
    getProductAutocomplete,
    getProducts,
    updateCategory,
    updateProduct,
  } from './lib/api';
  import CategoryNode from './lib/CategoryNode.svelte';
  import type {
    CategoryStatus,
    CategoryTreeItem,
    CategoryUpdatePayload,
    CategoryWritePayload,
    PagedResult,
    ProductAutocompleteItem,
    ProductDetail,
    ProductListItem,
    ProductUpdatePayload,
    ProductWritePayload,
  } from './lib/types';
  import './app.css';

  type AttributeKind = 'string' | 'number' | 'boolean' | 'json';
  type HealthTone = 'checking' | 'healthy' | 'degraded';

  interface AttributeRow {
    id: string;
    key: string;
    value: string;
    type: AttributeKind;
  }

  interface ProductFormState {
    id: number | null;
    rowVersion: string;
    name: string;
    description: string;
    price: string;
    inventoryOnHand: string;
    categoryId: string;
    primaryImageUrl: string;
    inventoryReason: string;
    changedBy: string;
  }

  interface CategoryFormState {
    id: number | null;
    rowVersion: string;
    name: string;
    description: string;
    parentCategoryId: string;
    status: CategoryStatus;
    displayOrder: string;
  }

  interface CategoryOption {
    id: number;
    label: string;
    status: CategoryStatus;
    depth: number;
  }

  interface HealthSnapshot {
    tone: HealthTone;
    message: string;
    checkedAt: string | null;
  }

  const emptyProducts: PagedResult<ProductListItem> = {
    items: [],
    pageSize: 12,
    totalCount: 0,
    cursor: null,
    nextCursor: null,
  };

  let ready = $state(false);

  let categories = $state<CategoryTreeItem[]>([]);
  let products = $state<PagedResult<ProductListItem>>(emptyProducts);
  let categoryProducts = $state<PagedResult<ProductListItem>>(emptyProducts);
  let selectedProduct = $state<ProductDetail | null>(null);
  let focusedCategoryId = $state<number | null>(null);
  let highlightedProductId = $state<number | null>(null);

  let categoriesBusy = $state(false);
  let productsBusy = $state(false);
  let categoryProductsBusy = $state(false);
  let productSaveBusy = $state(false);
  let categorySaveBusy = $state(false);
  let suggestionBusy = $state(false);

  let productsError = $state('');
  let categoryError = $state('');
  let categoryProductsError = $state('');
  let productMessage = $state('');
  let categoryMessage = $state('');
  let productErrors = $state<Record<string, string[]>>({});
  let categoryErrors = $state<Record<string, string[]>>({});

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

  let suggestions = $state<ProductAutocompleteItem[]>([]);
  let suggestionsOpen = $state(false);

  let health = $state<HealthSnapshot>({
    tone: 'checking',
    message: 'Checking API heartbeat…',
    checkedAt: null,
  });

  let productForm = $state(createEmptyProductForm());
  let categoryForm = $state(createEmptyCategoryForm());
  let attributeRows = $state<AttributeRow[]>([createAttributeRow()]);

  const categoryOptions = $derived(flattenCategories(categories));
  const activeCategoryOptions = $derived(categoryOptions.filter((category) => category.status === 'Active'));
  const activeCategoryCount = $derived(activeCategoryOptions.length);
  const inactiveCategoryCount = $derived(categoryOptions.length - activeCategoryCount);
  const focusedCategory = $derived(findCategoryById(categories, focusedCategoryId));
  const filteredCategory = $derived(findCategoryById(categories, categoryFilter ? Number(categoryFilter) : null));
  const selectedProductAttributes = $derived(getAttributeEntries(selectedProduct?.customAttributes ?? null));
  const currentPageLabel = $derived(
    products.totalCount === 0
      ? 'No products in the current query.'
      : `${products.items.length} loaded from ${products.totalCount} matching products`,
  );
  const querySummary = $derived(appliedSearch ? `Querying for "${appliedSearch}"` : 'Showing the full catalog canvas.');
  const categoryPulseLabel = $derived(
    focusedCategory
      ? `${categoryProducts.totalCount} products inside ${focusedCategory.name}`
      : 'Select a category to inspect its live product mix.',
  );

  onMount(() => {
    ready = true;
    void refreshCategories();
    void refreshHealth();

    const interval = window.setInterval(() => {
      void refreshHealth();
    }, 30000);

    return () => window.clearInterval(interval);
  });

  $effect(() => {
    if (!ready) {
      return;
    }

    void refreshProducts();
  });

  $effect(() => {
    if (!ready) {
      return;
    }

    void refreshCategoryProducts();
  });

  $effect(() => {
    if (!ready) {
      return;
    }

    const query = searchInput.trim();

    if (query.length < 2) {
      suggestions = [];
      suggestionsOpen = false;
      suggestionBusy = false;
      return;
    }

    suggestionBusy = true;
    const timeout = window.setTimeout(async () => {
      try {
        suggestions = await getProductAutocomplete({
          query,
          limit: 6,
        });
        suggestionsOpen = suggestions.length > 0;
      } catch {
        suggestions = [];
        suggestionsOpen = false;
      } finally {
        suggestionBusy = false;
      }
    }, 160);

    return () => window.clearTimeout(timeout);
  });

  async function refreshHealth() {
    health = {
      tone: 'checking',
      message: 'Checking API heartbeat…',
      checkedAt: health.checkedAt,
    };

    try {
      const status = await getHealth();
      health = {
        tone: 'healthy',
        message: status,
        checkedAt: new Date().toISOString(),
      };
    } catch (error) {
      health = {
        tone: 'degraded',
        message: getErrorMessage(error),
        checkedAt: new Date().toISOString(),
      };
    }
  }

  async function refreshCategories() {
    categoriesBusy = true;
    categoryError = '';

    try {
      const nextCategories = await getCategories();
      const nextOptions = flattenCategories(nextCategories);
      const nextActiveOptions = nextOptions.filter((category) => category.status === 'Active');

      categories = nextCategories;

      if (!focusedCategoryId || !findCategoryById(nextCategories, focusedCategoryId)) {
        focusedCategoryId = nextCategories[0]?.id ?? null;
      }

      if (categoryForm.id) {
        const refreshedCategory = findCategoryById(nextCategories, categoryForm.id);
        if (refreshedCategory) {
          categoryForm = categoryToForm(refreshedCategory);
        }
      }

      if (!productForm.id && !productForm.categoryId && nextActiveOptions.length > 0) {
        productForm.categoryId = String(nextActiveOptions[0].id);
      }
    } catch (error) {
      categoryError = getErrorMessage(error);
    } finally {
      categoriesBusy = false;
    }
  }

  async function refreshProducts() {
    productsBusy = true;
    productsError = '';

    try {
      products = await getProducts({
        query: appliedSearch || undefined,
        cursor,
        pageSize,
        categoryId: categoryFilter ? Number(categoryFilter) : null,
        priceFrom: toOptionalNumber(priceFromFilter),
        priceTo: toOptionalNumber(priceToFilter),
        sortBy,
        sortDir,
      });
    } catch (error) {
      productsError = getErrorMessage(error);
    } finally {
      productsBusy = false;
    }
  }

  async function refreshCategoryProducts() {
    if (!focusedCategoryId) {
      categoryProducts = emptyProducts;
      categoryProductsError = '';
      return;
    }

    categoryProductsBusy = true;
    categoryProductsError = '';

    try {
      categoryProducts = await getCategoryProducts(focusedCategoryId, {
        pageSize: 6,
        sortBy: 'updatedAt',
        sortDir: 'desc',
      });
    } catch (error) {
      categoryProductsError = getErrorMessage(error);
    } finally {
      categoryProductsBusy = false;
    }
  }

  async function loadProduct(id: number) {
    productsError = '';
    suggestionsOpen = false;

    try {
      const product = await getProduct(id);
      selectedProduct = product;
      highlightedProductId = product.id;
      productForm = productToForm(product);
      attributeRows = attributesToRows(product.customAttributes);
      productErrors = {};
      productMessage = `Loaded ${product.name} into the studio.`;
      focusedCategoryId = product.categoryId;
    } catch (error) {
      productsError = getErrorMessage(error);
    }
  }

  function beginCreateProduct() {
    selectedProduct = null;
    highlightedProductId = null;
    productForm = createEmptyProductForm(activeCategoryOptions[0]?.id);
    attributeRows = [createAttributeRow()];
    productErrors = {};
    productMessage = 'Drafting a new product.';
  }

  function applySearchFilter() {
    resetCursor();
    appliedSearch = searchInput.trim();
    suggestionsOpen = false;
  }

  function selectSuggestion(item: ProductAutocompleteItem) {
    searchInput = item.name;
    appliedSearch = item.name;
    resetCursor();
    suggestionsOpen = false;
    void loadProduct(item.id);
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
    suggestions = [];
    suggestionsOpen = false;
  }

  function resetCursor() {
    cursor = null;
    cursorHistory = [];
  }

  async function submitProductForm() {
    productSaveBusy = true;
    productErrors = {};
    productMessage = '';

    try {
      const payload = buildProductPayload();
      const isUpdate = productForm.id !== null;
      const saved = isUpdate
        ? await updateProduct(productForm.id!, {
            ...payload,
            rowVersion: productForm.rowVersion,
          } satisfies ProductUpdatePayload)
        : await createProduct(payload satisfies ProductWritePayload);

      selectedProduct = saved;
      highlightedProductId = saved.id;
      focusedCategoryId = saved.categoryId;
      productForm = productToForm(saved);
      attributeRows = attributesToRows(saved.customAttributes);
      productMessage = isUpdate
        ? `Saved changes to ${saved.name}.`
        : `Created ${saved.name}.`;

      await Promise.all([refreshProducts(), refreshCategoryProducts()]);
    } catch (error) {
      handleProductError(error);
    } finally {
      productSaveBusy = false;
    }
  }

  async function removeSelectedProduct() {
    if (!productForm.id) {
      return;
    }

    const confirmed = window.confirm(`Delete "${productForm.name}" from the catalog?`);
    if (!confirmed) {
      return;
    }

    productSaveBusy = true;
    productErrors = {};
    productMessage = '';

    try {
      await deleteProduct(productForm.id);
      productMessage = `Deleted ${productForm.name}.`;
      beginCreateProduct();
      await Promise.all([refreshProducts(), refreshCategoryProducts()]);
    } catch (error) {
      handleProductError(error);
    } finally {
      productSaveBusy = false;
    }
  }

  function focusCategory(category: CategoryTreeItem) {
    focusedCategoryId = category.id;
    categoryForm = categoryToForm(category);
    categoryErrors = {};
    categoryMessage = `Category studio focused on ${category.name}.`;
  }

  function beginCreateCategory(parentCategoryId: number | null = null) {
    categoryForm = createEmptyCategoryForm(parentCategoryId);
    categoryErrors = {};
    categoryMessage = parentCategoryId
      ? 'Drafting a child category.'
      : 'Drafting a new root category.';
  }

  async function submitCategoryForm() {
    categorySaveBusy = true;
    categoryErrors = {};
    categoryMessage = '';

    try {
      const payload = buildCategoryPayload();
      const isUpdate = categoryForm.id !== null;
      const saved = isUpdate
        ? await updateCategory(categoryForm.id!, {
            ...payload,
            rowVersion: categoryForm.rowVersion,
          } satisfies CategoryUpdatePayload)
        : await createCategory(payload satisfies CategoryWritePayload);

      focusedCategoryId = saved.id;
      categoryMessage = isUpdate
        ? `Saved ${saved.name}.`
        : `Created ${saved.name}.`;

      await refreshCategories();

      const refreshed = findCategoryById(categories, saved.id);
      categoryForm = refreshed ? categoryToForm(refreshed) : categoryToForm(saved);
    } catch (error) {
      handleCategoryError(error);
    } finally {
      categorySaveBusy = false;
    }
  }

  function addAttributeRow() {
    attributeRows = [...attributeRows, createAttributeRow()];
  }

  function removeAttributeRow(id: string) {
    const remaining = attributeRows.filter((row) => row.id !== id);
    attributeRows = remaining.length > 0 ? remaining : [createAttributeRow()];
  }

  function handleProductError(error: unknown) {
    if (error instanceof ApiError) {
      productErrors = error.errors ?? {};
      productMessage = error.message;
      return;
    }

    productMessage = getErrorMessage(error);
  }

  function handleCategoryError(error: unknown) {
    if (error instanceof ApiError) {
      categoryErrors = error.errors ?? {};
      categoryMessage = error.message;
      return;
    }

    categoryMessage = getErrorMessage(error);
  }

  function createEmptyProductForm(categoryId?: number): ProductFormState {
    return {
      id: null,
      rowVersion: '',
      name: '',
      description: '',
      price: '0.00',
      inventoryOnHand: '0',
      categoryId: categoryId ? String(categoryId) : '',
      primaryImageUrl: '',
      inventoryReason: '',
      changedBy: 'catalog-admin',
    };
  }

  function createEmptyCategoryForm(parentCategoryId?: number | null): CategoryFormState {
    return {
      id: null,
      rowVersion: '',
      name: '',
      description: '',
      parentCategoryId: parentCategoryId ? String(parentCategoryId) : '',
      status: 'Active',
      displayOrder: '10',
    };
  }

  function createAttributeRow(): AttributeRow {
    return {
      id: crypto.randomUUID(),
      key: '',
      value: '',
      type: 'string',
    };
  }

  function buildProductPayload(): ProductWritePayload {
    return {
      name: productForm.name.trim(),
      description: toNullableString(productForm.description),
      price: Number(productForm.price),
      inventoryOnHand: Number(productForm.inventoryOnHand),
      categoryId: Number(productForm.categoryId),
      primaryImageUrl: toNullableString(productForm.primaryImageUrl),
      customAttributes: buildCustomAttributes(),
      inventoryReason: toNullableString(productForm.inventoryReason),
      changedBy: toNullableString(productForm.changedBy),
    };
  }

  function buildCategoryPayload(): CategoryWritePayload {
    return {
      name: categoryForm.name.trim(),
      description: toNullableString(categoryForm.description),
      parentCategoryId: categoryForm.parentCategoryId ? Number(categoryForm.parentCategoryId) : null,
      status: categoryForm.status,
      displayOrder: Number(categoryForm.displayOrder),
    };
  }

  function buildCustomAttributes(): Record<string, unknown> | null {
    const entries = attributeRows
      .map((row) => ({
        key: row.key.trim(),
        type: row.type,
        value: row.value.trim(),
      }))
      .filter((row) => row.key.length > 0);

    if (entries.length === 0) {
      return null;
    }

    const attributes: Record<string, unknown> = {};

    for (const entry of entries) {
      if (entry.type === 'number') {
        attributes[entry.key] = Number(entry.value);
        continue;
      }

      if (entry.type === 'boolean') {
        attributes[entry.key] = entry.value.toLowerCase() === 'true';
        continue;
      }

      if (entry.type === 'json') {
        try {
          attributes[entry.key] = JSON.parse(entry.value);
        } catch {
          throw new Error(`Attribute "${entry.key}" must contain valid JSON.`);
        }

        continue;
      }

      attributes[entry.key] = entry.value;
    }

    return attributes;
  }

  function productToForm(product: ProductDetail): ProductFormState {
    return {
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

  function categoryToForm(category: CategoryTreeItem): CategoryFormState {
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

  function attributesToRows(attributes: Record<string, unknown> | null): AttributeRow[] {
    if (!attributes || Object.keys(attributes).length === 0) {
      return [createAttributeRow()];
    }

    return Object.entries(attributes).map(([key, value]) => {
      if (typeof value === 'number') {
        return { id: crypto.randomUUID(), key, value: String(value), type: 'number' as const };
      }

      if (typeof value === 'boolean') {
        return { id: crypto.randomUUID(), key, value: String(value), type: 'boolean' as const };
      }

      if (typeof value === 'string') {
        return { id: crypto.randomUUID(), key, value, type: 'string' as const };
      }

      return {
        id: crypto.randomUUID(),
        key,
        value: JSON.stringify(value),
        type: 'json' as const,
      };
    });
  }

  function flattenCategories(tree: CategoryTreeItem[], depth = 0): CategoryOption[] {
    return tree.flatMap((category) => [
      {
        id: category.id,
        label: `${depth > 0 ? `${'  '.repeat(depth)}↳ ` : ''}${category.name}`,
        status: category.status,
        depth,
      },
      ...flattenCategories(category.children, depth + 1),
    ]);
  }

  function findCategoryById(tree: CategoryTreeItem[], id: number | null): CategoryTreeItem | null {
    if (id === null) {
      return null;
    }

    for (const category of tree) {
      if (category.id === id) {
        return category;
      }

      const childMatch = findCategoryById(category.children, id);
      if (childMatch) {
        return childMatch;
      }
    }

    return null;
  }

  function getAttributeEntries(attributes: Record<string, unknown> | null): Array<[string, unknown]> {
    return attributes ? Object.entries(attributes) : [];
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 2,
    }).format(value);
  }

  function formatNumber(value: number) {
    return new Intl.NumberFormat('en-US').format(value);
  }

  function formatDate(value: string) {
    return new Date(value).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  function formatDateTime(value: string | null) {
    if (!value) {
      return 'Not checked yet';
    }

    return new Date(value).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  function inventoryMeter(value: number) {
    return `${Math.min(100, 16 + value * 3)}%`;
  }

  function renderAttributeValue(value: unknown) {
    return typeof value === 'string' ? value : JSON.stringify(value, null, 2);
  }

  function initials(name: string) {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  function goToPreviousPage() {
    if (cursorHistory.length === 0) {
      return;
    }

    const nextHistory = [...cursorHistory];
    const previousCursor = nextHistory.pop() ?? null;
    cursorHistory = nextHistory;
    cursor = previousCursor;
  }

  function goToNextPage() {
    if (!products.nextCursor) {
      return;
    }

    cursorHistory = [...cursorHistory, cursor];
    cursor = products.nextCursor;
  }

  function updatePageSize(event: Event) {
    pageSize = Number((event.currentTarget as HTMLSelectElement).value);
    resetCursor();
  }

  function toNullableString(value: string): string | null {
    const normalized = value.trim();
    return normalized.length > 0 ? normalized : null;
  }

  function toOptionalNumber(value: string): number | null {
    const normalized = value.trim();
    return normalized.length > 0 ? Number(normalized) : null;
  }

  function getErrorMessage(error: unknown): string {
    if (error instanceof ApiError) {
      return error.message;
    }

    if (error instanceof Error) {
      return error.message;
    }

    return 'Something went wrong while talking to the catalog API.';
  }

  function errorFor(errors: Record<string, string[]>, key: string) {
    return errors[key]?.[0] ?? '';
  }
</script>

<svelte:head>
  <title>Catalog Studio</title>
</svelte:head>

<div class="page-shell">
  <div class="ambient ambient-one"></div>
  <div class="ambient ambient-two"></div>
  <div class="ambient ambient-grid"></div>

  <header class="masthead">
    <div>
      <p class="eyebrow">Catalog Studio</p>
      <h1>Design-forward product operations for every server endpoint.</h1>
      <p class="lede">
        Search with autocomplete, curate products with revision-safe editors, shape category hierarchies, and inspect live category slices without leaving the workspace.
      </p>
    </div>

    <div class="masthead-actions">
      <div class:checking={health.tone === 'checking'} class:healthy={health.tone === 'healthy'} class:degraded={health.tone === 'degraded'} class="health-chip">
        <span class="health-dot"></span>
        <div>
          <strong>{health.tone === 'healthy' ? 'API healthy' : health.tone === 'checking' ? 'Checking API' : 'API needs attention'}</strong>
          <small>{health.message} · {formatDateTime(health.checkedAt)}</small>
        </div>
      </div>

      <div class="quick-links">
        <button class="ghost-button" type="button" onclick={() => void refreshHealth()}>Refresh health</button>
        <a class="ghost-link" href="/health" rel="noreferrer" target="_blank">Open health</a>
        <a class="ghost-link" href="/swagger" rel="noreferrer" target="_blank">Open Swagger</a>
      </div>
    </div>
  </header>

  <section class="hero-panel panel">
    <div class="hero-copy">
      <p class="panel-label">Command Deck</p>
      <h2>Find anything in the catalog before you touch the editors.</h2>
      <p>
        The command search is backed by the dedicated autocomplete endpoint, while the larger stream below stays tied to the paged product list endpoint.
      </p>

      <div class="command-search">
        <div class="command-input-wrap">
          <input
            bind:value={searchInput}
            placeholder="Search products by name, then load or query them…"
            onfocus={() => (suggestionsOpen = suggestions.length > 0)}
            onkeydown={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
                applySearchFilter();
              }
            }}
          />
          <button type="button" onclick={applySearchFilter}>Run query</button>

          {#if suggestionsOpen && (suggestions.length > 0 || suggestionBusy)}
            <div class="suggestions">
              {#if suggestionBusy}
                <p class="suggestion-empty">Looking for likely matches…</p>
              {:else}
                {#each suggestions as suggestion (suggestion.id)}
                  <button class="suggestion-item" type="button" onclick={() => selectSuggestion(suggestion)}>
                    <div class="suggestion-copy">
                      <strong>{suggestion.name}</strong>
                      <span>{suggestion.categoryName}</span>
                    </div>
                    <span class="suggestion-action">Load</span>
                  </button>
                {/each}
              {/if}
            </div>
          {/if}
        </div>
      </div>

      <div class="query-ribbon">
        <span>{querySummary}</span>
        <span>{currentPageLabel}</span>
      </div>
    </div>

    <div class="hero-metrics">
      <article>
        <span>{formatNumber(products.totalCount)}</span>
        <strong>Products in current query</strong>
        <p>Paged from `GET /api/products` with filters, sort, and pagination.</p>
      </article>
      <article>
        <span>{formatNumber(activeCategoryCount)}</span>
        <strong>Active categories</strong>
        <p>{inactiveCategoryCount} inactive categories remain visible for maintenance.</p>
      </article>
      <article>
        <span>{focusedCategory ? formatNumber(categoryProducts.totalCount) : 'Pick one'}</span>
        <strong>Category pulse</strong>
        <p>{categoryPulseLabel}</p>
      </article>
    </div>
  </section>

  <main class="workspace">
    <div class="catalog-stage">
      <section class="panel control-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Catalog Stream</p>
            <h2>Filter the live product feed.</h2>
          </div>
          <button type="button" onclick={beginCreateProduct}>New product</button>
        </div>

        <div class="query-grid">
          <label>
            <span>Category scope</span>
            <select bind:value={categoryFilter} onchange={resetCursor}>
              <option value="">All categories</option>
              {#each categoryOptions as option}
                <option value={option.id}>{option.label}</option>
              {/each}
            </select>
          </label>

          <label>
            <span>Sort by</span>
            <select bind:value={sortBy} onchange={resetCursor}>
              <option value="updatedAt">Recently updated</option>
              <option value="name">Name</option>
              <option value="price">Price</option>
              <option value="inventory">Inventory</option>
            </select>
          </label>

          <label>
            <span>Direction</span>
            <select bind:value={sortDir} onchange={resetCursor}>
              <option value="desc">Descending</option>
              <option value="asc">Ascending</option>
            </select>
          </label>

          <label>
            <span>Page size</span>
            <select value={String(pageSize)} onchange={updatePageSize}>
              <option value="6">6</option>
              <option value="12">12</option>
              <option value="24">24</option>
            </select>
          </label>

          <label>
            <span>Price floor</span>
            <input bind:value={priceFromFilter} min="0" step="0.01" placeholder="0.00" type="number" />
          </label>

          <label>
            <span>Price ceiling</span>
            <input bind:value={priceToFilter} min="0" step="0.01" placeholder="999.00" type="number" />
          </label>
        </div>

        <div class="filter-footer">
          <div class="active-filters">
            {#if appliedSearch}
              <span class="filter-chip">Query: {appliedSearch}</span>
            {/if}
            {#if filteredCategory}
              <span class="filter-chip">Category: {filteredCategory.name}</span>
            {/if}
            {#if priceFromFilter}
              <span class="filter-chip">From {formatMoney(Number(priceFromFilter))}</span>
            {/if}
            {#if priceToFilter}
              <span class="filter-chip">To {formatMoney(Number(priceToFilter))}</span>
            {/if}
          </div>

          <button class="ghost-button" type="button" onclick={resetCatalogFilters}>Reset</button>
        </div>

        {#if productsError}
          <p class="status error">{productsError}</p>
        {/if}
      </section>

      <section class="panel stream-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Products</p>
            <h2>Curated result stream</h2>
          </div>
          {#if productsBusy}
            <p class="status">Refreshing feed…</p>
          {/if}
        </div>

        <div class="product-grid">
          {#if products.items.length === 0 && !productsBusy}
            <div class="empty-state">
              <h3>No products matched this view.</h3>
              <p>Relax the filters, clear the query, or start a new product from the studio.</p>
            </div>
          {/if}

          {#each products.items as product (product.id)}
            <button
              class:selected={highlightedProductId === product.id}
              class="product-card"
              type="button"
              onclick={() => void loadProduct(product.id)}
            >
              <div class="product-card-media">
                {#if product.primaryImageUrl}
                  <img alt={product.name} loading="lazy" src={product.primaryImageUrl} />
                {:else}
                  <span>{initials(product.name)}</span>
                {/if}
              </div>

              <div class="product-card-copy">
                <div class="product-card-meta">
                  <span class="pill">{product.categoryName}</span>
                  <span class="version-pill">v{product.versionNumber}</span>
                </div>

                <h3>{product.name}</h3>
                <p>{product.description ?? 'No description provided yet.'}</p>

                <div class="inventory-track">
                  <span style={`width:${inventoryMeter(product.inventoryOnHand)}`}></span>
                </div>

                <div class="product-card-footer">
                  <strong>{formatMoney(product.price)}</strong>
                  <small>{product.inventoryOnHand} on hand · Updated {formatDate(product.updatedAtUtc)}</small>
                </div>
              </div>
            </button>
          {/each}
        </div>

        <div class="pagination">
          <button class="ghost-button" type="button" onclick={goToPreviousPage} disabled={cursorHistory.length === 0}>Previous</button>
          <span>Showing {products.items.length} of {products.totalCount}</span>
          <button
            class="ghost-button"
            type="button"
            onclick={goToNextPage}
            disabled={!products.nextCursor}
          >
            Next
          </button>
        </div>
      </section>

      <section class="panel category-pulse-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Category Pulse</p>
            <h2>{focusedCategory ? focusedCategory.name : 'Pick a category'}</h2>
          </div>
          {#if focusedCategory}
            <button class="ghost-button" type="button" onclick={() => beginCreateCategory(focusedCategory.id)}>Create child</button>
          {/if}
        </div>

        {#if focusedCategory}
          <p class="panel-copy">
            {focusedCategory.description ?? 'No category description yet.'}
          </p>

          <div class="category-facts">
            <span>Status {focusedCategory.status}</span>
            <span>Order {focusedCategory.displayOrder}</span>
            <span>Updated {formatDate(focusedCategory.updatedAtUtc)}</span>
          </div>
        {/if}

        {#if categoryProductsError}
          <p class="status error">{categoryProductsError}</p>
        {/if}

        <div class="category-product-grid">
          {#if !focusedCategory}
            <div class="empty-state">
              <h3>No category selected</h3>
              <p>Use the category atlas to inspect the dedicated `GET /api/categories/{'{id}'}/products` endpoint.</p>
            </div>
          {:else if categoryProducts.items.length === 0 && !categoryProductsBusy}
            <div class="empty-state">
              <h3>No products in this category</h3>
              <p>This category is ready for new inventory or a tighter parent grouping.</p>
            </div>
          {:else}
            {#each categoryProducts.items as product (product.id)}
              <button class="pulse-card" type="button" onclick={() => void loadProduct(product.id)}>
                <div>
                  <strong>{product.name}</strong>
                  <small>{formatMoney(product.price)} · Qty {product.inventoryOnHand}</small>
                </div>
                <span>Open</span>
              </button>
            {/each}
          {/if}
        </div>
      </section>
    </div>

    <div class="studio-stage">
      <section class="panel spotlight-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Product Spotlight</p>
            <h2>{selectedProduct ? selectedProduct.name : 'Select a product'}</h2>
          </div>
          {#if selectedProduct}
            <button class="ghost-button" type="button" onclick={() => void loadProduct(selectedProduct!.id)}>Reload detail</button>
          {/if}
        </div>

        {#if selectedProduct}
          <div class="spotlight-media">
            {#if selectedProduct.primaryImageUrl}
              <img alt={selectedProduct.name} src={selectedProduct.primaryImageUrl} />
            {:else}
              <div class="spotlight-fallback">{initials(selectedProduct.name)}</div>
            {/if}
          </div>

          <div class="spotlight-facts">
            <div>
              <span>Category</span>
              <strong>{selectedProduct.categoryName}</strong>
            </div>
            <div>
              <span>Version</span>
              <strong>v{selectedProduct.versionNumber}</strong>
            </div>
            <div>
              <span>Price</span>
              <strong>{formatMoney(selectedProduct.price)}</strong>
            </div>
            <div>
              <span>Inventory</span>
              <strong>{selectedProduct.inventoryOnHand}</strong>
            </div>
          </div>

          <p class="spotlight-description">
            {selectedProduct.description ?? 'No product description provided yet.'}
          </p>

          <div class="spotlight-timestamps">
            <span>Created {formatDate(selectedProduct.createdAtUtc)}</span>
            <span>Updated {formatDate(selectedProduct.updatedAtUtc)}</span>
          </div>

          <div class="attribute-panel">
            <div class="subheading">
              <h3>Flexible attributes</h3>
              <span>{selectedProductAttributes.length} mapped fields</span>
            </div>

            {#if selectedProductAttributes.length === 0}
              <p class="muted">This product does not currently expose custom attributes.</p>
            {:else}
              <div class="attribute-view">
                {#each selectedProductAttributes as [key, value] (key)}
                  <div class="attribute-chip">
                    <strong>{key}</strong>
                    <pre>{renderAttributeValue(value)}</pre>
                  </div>
                {/each}
              </div>
            {/if}
          </div>
        {:else}
          <div class="empty-state">
            <h3>No spotlight yet</h3>
            <p>Pick a result card, use autocomplete, or load a category product to inspect the dedicated detail endpoint.</p>
          </div>
        {/if}
      </section>

      <section class="panel editor-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Product Editor</p>
            <h2>{productForm.id ? 'Update product' : 'Create product'}</h2>
          </div>
          <div class="editor-actions">
            <button class="ghost-button" type="button" onclick={beginCreateProduct}>Reset draft</button>
            <button
              class="danger-button"
              type="button"
              onclick={() => void removeSelectedProduct()}
              disabled={!productForm.id || productSaveBusy}
            >
              Delete
            </button>
          </div>
        </div>

        {#if productMessage}
          <p class="status {Object.keys(productErrors).length > 0 ? 'error' : 'success'}">{productMessage}</p>
        {/if}

        <form class="editor-form" onsubmit={(event) => { event.preventDefault(); void submitProductForm(); }}>
          <div class="form-grid">
            <label>
              <span>Name</span>
              <input bind:value={productForm.name} placeholder="Aurora Desk Lamp" required />
              {#if errorFor(productErrors, 'Name')}
                <small>{errorFor(productErrors, 'Name')}</small>
              {/if}
            </label>

            <label>
              <span>Category</span>
              <select bind:value={productForm.categoryId} required>
                <option disabled value="">Select a category</option>
                {#each activeCategoryOptions as option}
                  <option value={option.id}>{option.label}</option>
                {/each}
              </select>
              {#if errorFor(productErrors, 'CategoryId')}
                <small>{errorFor(productErrors, 'CategoryId')}</small>
              {/if}
            </label>

            <label>
              <span>Price</span>
              <input bind:value={productForm.price} min="0" step="0.01" type="number" />
            </label>

            <label>
              <span>Inventory on hand</span>
              <input bind:value={productForm.inventoryOnHand} min="0" step="1" type="number" />
              {#if errorFor(productErrors, 'InventoryOnHand')}
                <small>{errorFor(productErrors, 'InventoryOnHand')}</small>
              {/if}
            </label>
          </div>

          <label>
            <span>Description</span>
            <textarea bind:value={productForm.description} placeholder="Describe the product, audience, and product feel." rows="4"></textarea>
          </label>

          <label>
            <span>Primary image URL</span>
            <input bind:value={productForm.primaryImageUrl} placeholder="https://images.example.com/product.jpg" />
            {#if errorFor(productErrors, 'PrimaryImageUrl')}
              <small>{errorFor(productErrors, 'PrimaryImageUrl')}</small>
            {/if}
          </label>

          <div class="subpanel">
            <div class="subheading">
              <h3>Custom attributes</h3>
              <button class="ghost-button" type="button" onclick={addAttributeRow}>Add attribute</button>
            </div>

            <div class="attribute-editor">
              {#each attributeRows as row (row.id)}
                <div class="attribute-row">
                  <input bind:value={row.key} placeholder="material" />
                  <select bind:value={row.type}>
                    <option value="string">Text</option>
                    <option value="number">Number</option>
                    <option value="boolean">Boolean</option>
                    <option value="json">JSON</option>
                  </select>
                  <input bind:value={row.value} placeholder={row.type === 'json' ? '{"finish":"matte"}' : 'aluminum'} />
                  <button class="ghost-button" type="button" onclick={() => removeAttributeRow(row.id)}>Remove</button>
                </div>
              {/each}
            </div>
          </div>

          <div class="form-grid">
            <label>
              <span>Inventory reason</span>
              <input bind:value={productForm.inventoryReason} placeholder="Cycle count, seasonal restock…" />
              {#if errorFor(productErrors, 'InventoryReason')}
                <small>{errorFor(productErrors, 'InventoryReason')}</small>
              {/if}
            </label>

            <label>
              <span>Changed by</span>
              <input bind:value={productForm.changedBy} placeholder="catalog-admin" />
              {#if errorFor(productErrors, 'ChangedBy')}
                <small>{errorFor(productErrors, 'ChangedBy')}</small>
              {/if}
            </label>
          </div>

          <button type="submit" disabled={productSaveBusy}>
            {productSaveBusy ? 'Saving…' : productForm.id ? 'Save product' : 'Create product'}
          </button>
        </form>
      </section>

      <section class="panel category-panel">
        <div class="section-heading">
          <div>
            <p class="panel-label">Category Atlas</p>
            <h2>Shape hierarchy, order, and status.</h2>
          </div>
          <button class="ghost-button" type="button" onclick={() => beginCreateCategory()}>New root category</button>
        </div>

        {#if categoryMessage}
          <p class="status {Object.keys(categoryErrors).length > 0 ? 'error' : 'success'}">{categoryMessage}</p>
        {/if}

        {#if categoryError}
          <p class="status error">{categoryError}</p>
        {/if}

        <div class="category-layout">
          <div class="category-tree">
            {#if categoriesBusy}
              <p class="status">Loading categories…</p>
            {/if}

            {#each categories as category (category.id)}
              <CategoryNode
                {category}
                currentId={focusedCategoryId}
                onCreateChild={beginCreateCategory}
                onSelect={focusCategory}
              />
            {/each}
          </div>

          <form class="editor-form" onsubmit={(event) => { event.preventDefault(); void submitCategoryForm(); }}>
            <div class="form-grid">
              <label>
                <span>Name</span>
                <input bind:value={categoryForm.name} placeholder="Ambient Lighting" required />
                {#if errorFor(categoryErrors, 'Name')}
                  <small>{errorFor(categoryErrors, 'Name')}</small>
                {/if}
              </label>

              <label>
                <span>Parent category</span>
                <select bind:value={categoryForm.parentCategoryId}>
                  <option value="">Root category</option>
                  {#each categoryOptions.filter((option) => option.id !== categoryForm.id) as option}
                    <option value={option.id}>{option.label}</option>
                  {/each}
                </select>
                {#if errorFor(categoryErrors, 'ParentCategoryId')}
                  <small>{errorFor(categoryErrors, 'ParentCategoryId')}</small>
                {/if}
              </label>

              <label>
                <span>Status</span>
                <select bind:value={categoryForm.status}>
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </label>

              <label>
                <span>Display order</span>
                <input bind:value={categoryForm.displayOrder} min="0" step="1" type="number" />
                {#if errorFor(categoryErrors, 'DisplayOrder')}
                  <small>{errorFor(categoryErrors, 'DisplayOrder')}</small>
                {/if}
              </label>
            </div>

            <label>
              <span>Description</span>
              <textarea bind:value={categoryForm.description} placeholder="Define what belongs here and what should stay out." rows="4"></textarea>
            </label>

            <button type="submit" disabled={categorySaveBusy}>
              {categorySaveBusy ? 'Saving…' : categoryForm.id ? 'Save category' : 'Create category'}
            </button>
          </form>
        </div>
      </section>
    </div>
  </main>
</div>
