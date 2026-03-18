<script lang="ts">
  import '../app.css';
  import { page } from '$app/stores';
  import { LayoutDashboard, PackageSearch, Folders } from 'lucide-svelte';
  import { QueryClient, QueryClientProvider } from '@tanstack/svelte-query';
  import * as Sidebar from '$lib/components/ui/sidebar/index.js';

  let { children } = $props();

  const queryClient = new QueryClient();
</script>

<QueryClientProvider client={queryClient}>
  <Sidebar.Provider>
    <Sidebar.Root>
      <Sidebar.Header class="p-4 border-b">
        <div class="flex items-center gap-2 px-2">
          <div class="flex h-6 w-6 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <PackageSearch class="h-4 w-4" />
          </div>
          <span class="font-semibold select-none">Catalog Studio</span>
        </div>
      </Sidebar.Header>
      
      <Sidebar.Content>
        <Sidebar.Group>
          <Sidebar.GroupLabel>Navigation</Sidebar.GroupLabel>
          <Sidebar.GroupContent>
            <Sidebar.Menu>
              <Sidebar.MenuItem>
                <Sidebar.MenuButton isActive={$page.url.pathname === '/'} class="font-medium">
                  {#snippet child({ props })}
                    <a href="/" {...props}>
                      <LayoutDashboard class="h-4 w-4" />
                      <span>Dashboard</span>
                    </a>
                  {/snippet}
                </Sidebar.MenuButton>
              </Sidebar.MenuItem>
              <Sidebar.MenuItem>
                <Sidebar.MenuButton isActive={$page.url.pathname.startsWith('/products')} class="font-medium">
                  {#snippet child({ props })}
                    <a href="/products" {...props}>
                      <PackageSearch class="h-4 w-4" />
                      <span>Products</span>
                    </a>
                  {/snippet}
                </Sidebar.MenuButton>
              </Sidebar.MenuItem>
              <Sidebar.MenuItem>
                <Sidebar.MenuButton isActive={$page.url.pathname.startsWith('/categories')} class="font-medium">
                  {#snippet child({ props })}
                    <a href="/categories" {...props}>
                      <Folders class="h-4 w-4" />
                      <span>Categories</span>
                    </a>
                  {/snippet}
                </Sidebar.MenuButton>
              </Sidebar.MenuItem>
            </Sidebar.Menu>
          </Sidebar.GroupContent>
        </Sidebar.Group>
      </Sidebar.Content>
      
      <Sidebar.Footer class="border-t p-4">
        <div class="flex items-center gap-2 text-sm text-muted-foreground px-2">
          <div class="h-2 w-2 rounded-full bg-green-500"></div>
          <span class="font-medium">System Online</span>
        </div>
      </Sidebar.Footer>
      <Sidebar.Rail />
    </Sidebar.Root>

    <Sidebar.Inset>
      <header class="flex h-14 items-center gap-4 border-b bg-background px-6 lg:h-[60px]">
        <Sidebar.Trigger />
      </header>
      <main class="flex-1 bg-background text-foreground h-[calc(100vh-60px)] overflow-y-auto">
        <div class="w-full h-full p-6 md:p-8 max-w-7xl mx-auto">
          {@render children()}
        </div>
      </main>
    </Sidebar.Inset>
  </Sidebar.Provider>
</QueryClientProvider>
