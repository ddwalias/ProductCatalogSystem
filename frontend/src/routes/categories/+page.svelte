<script lang="ts">
  import {
    createQuery,
    createMutation,
    useQueryClient,
  } from "@tanstack/svelte-query";
  import {
    getCategories,
    getCategoryProducts,
    createCategory,
    updateCategory,
  } from "../../lib/api";
  import CategoryNode from "../../lib/CategoryNode.svelte";
  import type {
    CategoryTreeItem,
    CategoryWritePayload,
    CategoryUpdatePayload,
  } from "../../lib/types";
  import {
    Folders,
    Plus,
    Layers,
    RefreshCw,
    BarChart,
    ChevronRight,
  } from "lucide-svelte";
  import { dndzone } from "svelte-dnd-action";
  import type { DndEvent } from "svelte-dnd-action";
  import { flip } from "svelte/animate";

  // Shadcn Components
  import { Button } from "$lib/components/ui/button/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import * as Form from "$lib/components/ui/form/index.js";
  import { Textarea } from "$lib/components/ui/textarea/index.js";
  import * as Card from "$lib/components/ui/card/index.js";
  import * as Select from "$lib/components/ui/select/index.js";

  import { superForm, defaults } from "sveltekit-superforms";
  import { zod4 } from "sveltekit-superforms/adapters";
  import { z } from "zod";

  const formSchema = z.object({
    id: z.number().nullable().optional(),
    rowVersion: z.string().optional(),
    name: z.string().min(1, "Name is required"),
    parentCategoryId: z.string().optional(),
    status: z.enum(["Active", "Inactive"]),
    displayOrder: z.coerce
      .number()
      .min(0, "Order must be 0 or greater")
      .default(10),
    description: z.string().optional(),
  });

  const queryClient = useQueryClient();

  const categoriesQuery = createQuery(() => ({
    queryKey: ["categories"],
    queryFn: getCategories,
  }));

  let focusedCategoryId = $state<number | null>(null);

  let rootCategories = $state<CategoryTreeItem[]>([]);
  $effect(() => {
    if (categoriesQuery.data) {
      rootCategories = [...categoriesQuery.data];
    }
  });

  let focusedCategory = $derived.by(() => {
    return findCategoryById(categoriesQuery.data || [], focusedCategoryId);
  });

  let categoryOptions = $derived.by(() => {
    return flattenCategories(categoriesQuery.data || []);
  });

  const categoryProductsQuery = createQuery(() => ({
    queryKey: ["categoryProducts", focusedCategoryId],
    queryFn: () =>
      getCategoryProducts(focusedCategoryId!, {
        pageSize: 6,
        sortBy: "updatedAt",
        sortDir: "desc",
      }),
    enabled: !!focusedCategoryId,
  }));

  let categoryMessage = $state("");
  let isError = $state(false);

  const createCatMutation = createMutation(() => ({
    mutationFn: createCategory,
    onSuccess: (saved) => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      focusCategory(saved);
      categoryMessage = `Created ${saved.name}.`;
      isError = false;
    },
    onError: (error) => {
      categoryMessage =
        error instanceof Error ? error.message : "Error creating category";
      isError = true;
    },
  }));

  const updateCatMutation = createMutation(() => ({
    mutationFn: ({
      id,
      payload,
    }: {
      id: number;
      payload: CategoryUpdatePayload;
    }) => updateCategory(id, payload),
    onSuccess: (saved) => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      focusCategory(saved);
      categoryMessage = `Saved ${saved.name}.`;
      isError = false;
    },
    onError: (error) => {
      categoryMessage =
        error instanceof Error ? error.message : "Error updating category";
      isError = true;
    },
  }));

  type FormSchema = z.infer<typeof formSchema>;

  const initialData: FormSchema = {
    id: null,
    rowVersion: "",
    name: "",
    description: "",
    parentCategoryId: "",
    status: "Active",
    displayOrder: 10,
  };

  const form = superForm(initialData, {
    SPA: true,
    validators: zod4(formSchema as any),
    onUpdate({ form: f }) {
      if (f.valid) {
        const data = f.data as FormSchema;
        const payload = {
          name: data.name.trim(),
          description: data.description?.trim() || null,
          parentCategoryId: data.parentCategoryId
            ? Number(data.parentCategoryId)
            : null,
          status: data.status,
          displayOrder: Number(data.displayOrder),
        };

        if (data.id) {
          updateCatMutation.mutate({
            id: data.id,
            payload: {
              ...payload,
              rowVersion: data.rowVersion ?? "",
            } as CategoryUpdatePayload,
          });
        } else {
          createCatMutation.mutate(payload as CategoryWritePayload);
        }
      }
    },
  });

  const { form: formData, enhance } = form;

  // Auto-focus first category if nothing is selected
  $effect(() => {
    if (
      !focusedCategoryId &&
      categoriesQuery.data &&
      categoriesQuery.data.length > 0
    ) {
      focusCategory(categoriesQuery.data[0]);
    }
  });

  function focusCategory(category: CategoryTreeItem) {
    focusedCategoryId = category.id;
    formData.set(categoryToForm(category));
    categoryMessage = `Focused on ${category.name}`;
    isError = false;
  }

  type ReorderEvent = CustomEvent<DndEvent<CategoryTreeItem>>;

  function handleRootConsider(e: ReorderEvent) {
    rootCategories = e.detail.items;
  }

  function handleRootFinalize(e: ReorderEvent) {
    rootCategories = e.detail.items;
    handleReorder(null, rootCategories, Number(e.detail.info.id));
  }

  function handleReorder(
    parentId: number | null,
    newItems: CategoryTreeItem[],
    movedCategoryId: number,
  ) {
    const movedIndex = newItems.findIndex(
      (item) => item.id === movedCategoryId,
    );
    if (movedIndex === -1) {
      return;
    }

    const movedItem = newItems[movedIndex];
    const previousSibling = movedIndex > 0 ? newItems[movedIndex - 1] : null;
    const nextSibling =
      movedIndex < newItems.length - 1 ? newItems[movedIndex + 1] : null;
    const newOrder = computeDisplayOrder(
      previousSibling?.displayOrder ?? null,
      nextSibling?.displayOrder ?? null,
    );

    if (
      movedItem.displayOrder === newOrder &&
      movedItem.parentCategoryId === parentId
    ) {
      return;
    }

    updateCatMutation.mutate({
      id: movedItem.id,
      payload: {
        parentCategoryId: parentId,
        displayOrder: newOrder,
        rowVersion: movedItem.rowVersion,
      },
    });
  }

  function computeDisplayOrder(
    previousOrder: number | null,
    nextOrder: number | null,
  ) {
    if (previousOrder === null && nextOrder === null) {
      return 10;
    }

    if (previousOrder === null) {
      return roundDisplayOrder(nextOrder! / 2);
    }

    if (nextOrder === null) {
      return roundDisplayOrder(previousOrder + 10);
    }

    return roundDisplayOrder((previousOrder + nextOrder) / 2);
  }

  function roundDisplayOrder(value: number) {
    return Number(value.toFixed(8));
  }

  function beginCreateCategory(parentCategoryId: number | null = null) {
    formData.set({
      id: null,
      rowVersion: "",
      name: "",
      description: "",
      parentCategoryId: parentCategoryId ? String(parentCategoryId) : "",
      status: "Active",
      displayOrder: 10,
    });
    categoryMessage = parentCategoryId
      ? "Drafting a child category."
      : "Drafting a new root category.";
    isError = false;
  }

  // Helpers
  function findCategoryById(
    tree: CategoryTreeItem[],
    id: number | null,
  ): CategoryTreeItem | null {
    if (id === null) return null;
    for (const category of tree) {
      if (category.id === id) return category;
      const childMatch = findCategoryById(category.children, id);
      if (childMatch) return childMatch;
    }
    return null;
  }

  function flattenCategories(
    tree: CategoryTreeItem[],
    depth = 0,
  ): { id: number; label: string }[] {
    return tree.flatMap((cat: CategoryTreeItem) => [
      {
        id: cat.id,
        label: `${depth > 0 ? `${"--".repeat(depth)} ` : ""}${cat.name}`,
      },
      ...flattenCategories(cat.children, depth + 1),
    ]);
  }

  function categoryToForm(category: CategoryTreeItem) {
    return {
      id: category.id,
      rowVersion: category.rowVersion,
      name: category.name,
      description: category.description ?? "",
      parentCategoryId: category.parentCategoryId
        ? String(category.parentCategoryId)
        : "",
      status: category.status,
      displayOrder:
        typeof category.displayOrder === "number"
          ? category.displayOrder
          : Number(category.displayOrder),
    };
  }

  function formatMoney(value: number) {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(value);
  }
</script>

<svelte:head>
  <title>Categories - Catalog Studio</title>
</svelte:head>

<div class="space-y-6">
  <div
    class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
  >
    <div>
      <h2 class="text-3xl font-bold tracking-tight">Categories</h2>
      <p class="text-muted-foreground mt-1">
        Organize products into hierarchical structures.
      </p>
    </div>
    <Button onclick={() => beginCreateCategory()}>
      <Plus class="mr-2 h-4 w-4" /> Root Category
    </Button>
  </div>

  <div class="flex flex-col xl:flex-row gap-6 items-start">
    <div class="flex-1 w-full flex flex-col gap-6">
      <Card.Root class="min-h-[400px]">
        <Card.Header class="pb-3 border-b bg-muted/20">
          <Card.Title
            class="flex items-center gap-2 text-sm font-semibold text-muted-foreground"
          >
            <Layers class="h-4 w-4" /> Hierarchy
          </Card.Title>
        </Card.Header>

        <Card.Content class="p-0 overflow-y-auto">
          {#if categoriesQuery.isPending}
            <div
              class="flex flex-col items-center justify-center p-12 text-muted-foreground gap-3"
            >
              <RefreshCw class="animate-spin h-8 w-8 text-primary" />
              <p>Loading tree...</p>
            </div>
          {:else if categoriesQuery.data?.length === 0}
            <div
              class="flex flex-col items-center justify-center p-12 text-muted-foreground gap-3"
            >
              <Folders class="h-10 w-10 opacity-50" />
              <p>No categories defined.</p>
            </div>
          {:else}
            <div class="p-4">
              <div
                class="space-y-1 pb-10 min-h-[60px] rounded-md transition-colors"
                use:dndzone={{
                  items: rootCategories,
                  type: "category",
                  flipDurationMs: 200,
                  dropTargetStyle: {
                    outline: "2px solid hsl(var(--primary))",
                    outlineOffset: "2px",
                    background: "hsl(var(--primary) / 0.1)",
                    borderRadius: "0.375rem",
                  },
                }}
                onconsider={handleRootConsider}
                onfinalize={handleRootFinalize}
              >
                {#each rootCategories as category (category.id)}
                  <div animate:flip={{ duration: 200 }}>
                    <CategoryNode
                      {category}
                      currentId={focusedCategoryId}
                      onCreateChild={beginCreateCategory}
                      onSelect={focusCategory}
                      onReorder={handleReorder}
                    />
                  </div>
                {/each}
                {#if rootCategories.length === 0}
                  <div
                    class="text-sm text-muted-foreground/50 text-center py-4 pointer-events-none select-none"
                  >
                    Drop root categories here...
                  </div>
                {/if}
              </div>
            </div>
          {/if}
        </Card.Content>
      </Card.Root>

      {#if focusedCategory}
        <Card.Root>
          <Card.Header class="pb-3 border-b bg-muted/20">
            <Card.Title
              class="flex items-center gap-2 text-sm font-semibold text-muted-foreground"
            >
              <BarChart class="h-4 w-4" />{focusedCategory.name}
            </Card.Title>
          </Card.Header>
          <Card.Content class="p-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
              <div class="p-3 bg-background border rounded-md shadow-sm">
                <div
                  class="text-muted-foreground text-xs uppercase font-semibold mb-1"
                >
                  Status
                </div>
                <div
                  class="font-bold {focusedCategory.status === 'Active'
                    ? 'text-green-600 dark:text-green-400'
                    : 'text-muted-foreground'}"
                >
                  {focusedCategory.status}
                </div>
              </div>
              <div class="p-3 bg-background border rounded-md shadow-sm">
                <div
                  class="text-muted-foreground text-xs uppercase font-semibold mb-1"
                >
                  Display Order
                </div>
                <div class="font-bold text-foreground">
                  {focusedCategory.displayOrder}
                </div>
              </div>
            </div>

            <h4 class="text-sm font-medium text-muted-foreground mb-3">
              Immediate Products
            </h4>
            {#if categoryProductsQuery.isPending}
              <div class="flex items-center justify-center h-16">
                <RefreshCw class="animate-spin h-5 w-5 text-muted-foreground" />
              </div>
            {:else if categoryProductsQuery.data?.items.length === 0}
              <div
                class="text-sm text-muted-foreground bg-muted/30 border border-dashed rounded-md p-6 text-center"
              >
                No products directly map to {focusedCategory.name}.
              </div>
            {:else}
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {#each categoryProductsQuery.data?.items || [] as prod}
                  <a
                    href="/products?search={encodeURIComponent(prod.name)}"
                    class="group flex justify-between items-center p-3 border shadow-sm rounded-md bg-background hover:bg-muted transition-colors"
                  >
                    <div class="min-w-0 pr-4">
                      <h4
                        class="text-sm font-medium text-foreground truncate group-hover:text-primary transition-colors"
                      >
                        {prod.name}
                      </h4>
                      <p class="text-xs text-muted-foreground mt-0.5">
                        {formatMoney(prod.price)} &middot; {prod.inventoryOnHand}
                        Qty
                      </p>
                    </div>
                    <ChevronRight
                      class="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors shrink-0"
                    />
                  </a>
                {/each}
              </div>
            {/if}
          </Card.Content>
        </Card.Root>
      {/if}
    </div>

    <div class="w-full xl:w-96 shrink-0">
      <Card.Root class="sticky top-8">
        <Card.Header>
          <Card.Title
            >{$formData.id ? "Category Editor" : "New Category"}</Card.Title
          >
          <Card.Description>
            {$formData.id
              ? "Modify the selected hierarchy item."
              : "Draft a new category into the structure."}
          </Card.Description>
        </Card.Header>
        <Card.Content>
          {#if categoryMessage}
            <div
              class="mb-6 p-3 rounded-md text-sm border shadow-sm {isError
                ? 'bg-destructive/10 text-destructive border-destructive/20'
                : 'bg-green-500/10 text-green-600 dark:text-green-400 border-green-500/20'}"
            >
              {categoryMessage}
            </div>
          {/if}

          <form method="POST" use:enhance class="space-y-5">
            <Form.Field {form} name="name">
              <Form.Control>
                {#snippet children({ props })}
                  <Form.Label>Name</Form.Label>
                  <Input
                    {...props}
                    bind:value={$formData.name}
                    placeholder="E.g. Electronics"
                  />
                {/snippet}
              </Form.Control>
              <Form.FieldErrors />
            </Form.Field>

            <Form.Field {form} name="parentCategoryId">
              <Form.Control>
                {#snippet children({ props })}
                  <Form.Label>Parent</Form.Label>
                  <Select.Root
                    type="single"
                    bind:value={$formData.parentCategoryId}
                    name={props.name}
                  >
                    <Select.Trigger {...props} class="w-full">
                      {$formData.parentCategoryId
                        ? (categoryOptions.find(
                            (o) => String(o.id) === $formData.parentCategoryId,
                          )?.label ?? "-- Root --")
                        : "-- Root --"}
                    </Select.Trigger>
                    <Select.Content>
                      <Select.Item value="">-- Root --</Select.Item>
                      {#each categoryOptions.filter((o) => o.id !== $formData.id) as option}
                        <Select.Item value={String(option.id)}
                          >{option.label}</Select.Item
                        >
                      {/each}
                    </Select.Content>
                  </Select.Root>
                {/snippet}
              </Form.Control>
              <Form.FieldErrors />
            </Form.Field>

            <div class="grid grid-cols-2 gap-4">
              <Form.Field {form} name="status">
                <Form.Control>
                  {#snippet children({ props })}
                    <Form.Label>Status</Form.Label>
                    <Select.Root
                      type="single"
                      bind:value={$formData.status}
                      name={props.name}
                    >
                      <Select.Trigger {...props} class="w-full">
                        {$formData.status === "Active" ? "Active" : "Inactive"}
                      </Select.Trigger>
                      <Select.Content>
                        <Select.Item value="Active">Active</Select.Item>
                        <Select.Item value="Inactive">Inactive</Select.Item>
                      </Select.Content>
                    </Select.Root>
                  {/snippet}
                </Form.Control>
                <Form.FieldErrors />
              </Form.Field>

              <Form.Field {form} name="displayOrder">
                <Form.Control>
                  {#snippet children({ props })}
                    <Form.Label>Order Position</Form.Label>
                    <Input
                      {...props}
                      type="number"
                      step="0.00000001"
                      bind:value={$formData.displayOrder}
                    />
                  {/snippet}
                </Form.Control>
                <Form.FieldErrors />
              </Form.Field>
            </div>

            <Form.Field {form} name="description">
              <Form.Control>
                {#snippet children({ props })}
                  <Form.Label>Description</Form.Label>
                  <Textarea
                    {...props}
                    bind:value={$formData.description}
                    rows={3}
                    placeholder="Hierarchy notes..."
                    class="resize-none"
                  />
                {/snippet}
              </Form.Control>
              <Form.FieldErrors />
            </Form.Field>

            <div class="pt-2">
              <Form.Button
                type="submit"
                disabled={createCatMutation.isPending ||
                  updateCatMutation.isPending}
                class="w-full"
              >
                {createCatMutation.isPending || updateCatMutation.isPending
                  ? "Saving..."
                  : "Save Settings"}
              </Form.Button>
            </div>
          </form>
        </Card.Content>
      </Card.Root>
    </div>
  </div>
</div>
