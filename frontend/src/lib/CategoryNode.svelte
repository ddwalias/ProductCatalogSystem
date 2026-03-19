<script lang="ts">
  import CategoryNode from './CategoryNode.svelte';
  import type { CategoryTreeItem } from './types';
  import { GitBranch, ChevronRight, ChevronDown, GripVertical } from 'lucide-svelte';
  import { slide } from 'svelte/transition';
  import { flip } from 'svelte/animate';
  import { dndzone } from 'svelte-dnd-action';

  interface Props {
    category: CategoryTreeItem;
    currentId: number | null;
    depth?: number;
    onSelect: (category: CategoryTreeItem) => void;
    onCreateChild: (parentCategoryId?: number | null) => void;
    onReorder?: (parentId: number | null, newItems: CategoryTreeItem[]) => void;
  }

  let { category, currentId, depth = 0, onSelect, onCreateChild, onReorder }: Props = $props();

  let isOpen = $state(true);

  let childrenItems = $state<CategoryTreeItem[]>([]);
  $effect(() => {
    childrenItems = [...category.children];
  });

  function handleConsider(e: CustomEvent<any>) {
    childrenItems = e.detail.items;
  }

  function handleFinalize(e: CustomEvent<any>) {
    childrenItems = e.detail.items;
    if (onReorder) {
      onReorder(category.id, childrenItems);
    }
  }
</script>

<div class="w-full">
  <div class="group flex items-center justify-between py-1.5 px-2 hover:bg-muted/50 rounded-md cursor-pointer transition-colors {currentId === category.id ? 'bg-accent text-accent-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'}"
       onclick={() => onSelect(category)}
       onkeydown={(e) => e.key === 'Enter' && onSelect(category)}
       role="button"
       tabindex="0">
    
    <div class="flex items-center gap-2 overflow-hidden" style="padding-left: {depth * 1.2}rem;">
      <div class="cursor-grab active:cursor-grabbing text-muted-foreground/40 hover:text-foreground transition-colors p-0.5">
        <GripVertical class="h-4 w-4" />
      </div>

      <button 
        type="button"
        class="p-0.5 hover:bg-muted rounded-md text-muted-foreground shrink-0 transition-colors {category.children.length === 0 ? 'invisible' : ''}"
        onclick={(e) => { e.stopPropagation(); isOpen = !isOpen; }}
        aria-label="Toggle children"
      >
        {#if isOpen}
          <ChevronDown class="h-4 w-4" />
        {:else}
          <ChevronRight class="h-4 w-4" />
        {/if}
      </button>

      <div class="relative flex items-center shrink-0">
        {#if category.status === 'Inactive'}
          <div class="w-2 h-2 rounded-full border border-muted-foreground/40 bg-muted"></div>
        {:else}
          <div class="w-2 h-2 rounded-full bg-primary/70 shadow-[0_0_4px_rgba(var(--primary),0.3)]"></div>
        {/if}
      </div>
      
      <div class="flex flex-col min-w-0">
        <span class="text-sm font-medium truncate {currentId === category.id ? 'text-accent-foreground' : 'text-foreground'}">{category.name}</span>
      </div>
    </div>

    <!-- Actions, shown on hover -->
    <div class="flex items-center gap-2 invisible group-hover:visible pr-1">
      <span class="text-[10px] text-muted-foreground uppercase tracking-wider">{category.children.length} sub &middot; Ord {category.displayOrder}</span>
      <button 
        type="button"
        title="Add Child Category"
        class="text-muted-foreground hover:text-primary hover:bg-primary/10 rounded-md transition-colors p-1" 
        onclick={(e) => { e.stopPropagation(); onCreateChild(category.id); }}
        aria-label="Add child category"
      >
        <GitBranch class="h-3.5 w-3.5" />
      </button>
    </div>
  </div>

  {#if isOpen}
    <div transition:slide={{ duration: 150 }} class="relative mt-0.5">
      <div class="absolute top-0 bottom-0 border-l border-border/60 pointer-events-none" style="left: {depth * 1.2 + 0.6 + 0.5 + 1.2}rem;"></div>
      <div class="flex flex-col gap-1 min-h-[30px] rounded-md transition-colors"
           use:dndzone={{ items: childrenItems, type: 'category', flipDurationMs: 200, dropTargetStyle: { outline: '2px solid hsl(var(--primary))', outlineOffset: '2px', background: 'hsl(var(--primary) / 0.1)', borderRadius: '0.375rem' } }}
           onconsider={handleConsider}
           onfinalize={handleFinalize}>
        {#each childrenItems as child (child.id)}
          <div animate:flip={{ duration: 200 }}>
            <CategoryNode
              category={child}
              currentId={currentId}
              depth={depth + 1}
              {onCreateChild}
              {onSelect}
              {onReorder}
            />
          </div>
        {/each}
        {#if childrenItems.length === 0}
          <div class="text-[10px] text-muted-foreground/50 uppercase tracking-widest pl-8 py-1 pointer-events-none select-none">Drop nested items here...</div>
        {/if}
      </div>
    </div>
  {/if}
</div>
