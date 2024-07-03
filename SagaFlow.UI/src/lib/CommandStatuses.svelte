<svelte:options customElement={{
    tag: "sf-command-statuses",
    shadow: "none",
    extend: extendToGetWebComponentRoot
}} />
<script lang="ts">
    import sagaFlow, {defaultSagaFlowServer, type ISagaFlowServerState} from "../state/SagaFlowState";
    import type {Readable} from "svelte/store";
    import Paging from "$lib/Paging.svelte";
    import debounce from "lodash/debounce";
    import {createWebComponentEventDispatcher, extendToGetWebComponentRoot} from "$lib/RootWebComponentEventDispatcher";
    import {type SagaFlowCommandStatus, Status} from "$lib/Models";
    
    // When used as a web-component, this is required to dispatch events from the web component
    export let __webComponentElement: HTMLElement | undefined = undefined;
    
    export let serverKey: string = defaultSagaFlowServer;
    
    export let pageIndex: number = 0;
    export let pageSize: number = 20;
    export let keyword: string = "";
    
    export let searchPlaceholderText: string = "search ..."

    let selectedCommandStatus : SagaFlowCommandStatus | undefined = undefined;
    
    let store: Readable<ISagaFlowServerState>;
    
    const dispatcher = createWebComponentEventDispatcher(__webComponentElement)
    
    const getStatusesDebounced = debounce(
        (pageIndex: number, pageSize: number, keyword: string, serverKey: string) => sagaFlow.getStatuses(pageIndex, pageSize, keyword, serverKey), 
        300
    );

    const onSelectCommand = (item: SagaFlowCommandStatus) => {
        selectedCommandStatus = item;
        
        dispatcher(
            "sf-command-status-selected",
            item
        );
    }
    
    $: getStatusesDebounced(pageIndex, pageSize, keyword, serverKey);
    $: store = sagaFlow.state(serverKey);
</script>

<table class="command-statuses">
    <caption class="command-statuses-search-container">
        <input class="command-statuses-search" type="search" placeholder={searchPlaceholderText} bind:value={keyword} />
    </caption>
    <thead>
        <tr>
            <td class="command-status-col icon-col"></td>
            <th class="command-status-col command-col">
                Command
            </th>
            <th class="command-status-col name-col">
                Name
            </th>
            <th class="command-status-col user-col">
                User
            </th>
            <th class="command-status-col start-col">
                Start
            </th>
            <th class="command-status-col finish-col">
                Finish
            </th>
            <th class="command-status-col progress-col">
                Progress
            </th>
        </tr>
    </thead>
    
    <tbody>
    {#if !$store.hasCommandStatuses}
            <tr>
                <td colspan="7" class="loading">
                    loading ...
                </td>
            </tr>
    {:else}
        {#each $store.commandStatuses.page as item}
            <tr class={`command-status-row ${item.commandName}`}
                class:command-status-row-started={item.status === Status.Started && !item.lastError}
                class:command-status-row-processing={item.status === Status.Processing && !item.lastError}
                class:command-status-row-errored={item.status === Status.Errored || item.lastError}
                class:command-status-row-completed={item.status === Status.Completed && !item.lastError}
                
                class:command-status-row-selected={item.sagaFlowCommandId === selectedCommandStatus?.sagaFlowCommandId}
                
                title={item.lastError}
                
                on:click={() => onSelectCommand(item)}
            >
                <td class="command-status-col icon-col">
                    <i />
                </td>
                <td class="command-status-col command-col">
                    {item.commandName}
                </td>
                <td class="command-status-col name-col">
                    {item.name}
                </td>
                <td class="command-status-col user-col">
                    {item.initiatingUser}
                </td>
                <td class="command-status-col start-col">
                    {item.startDateTime.toLocaleDateString()} {item.startDateTime.toLocaleTimeString()}
                </td>
                <td class="command-status-col finish-col">
                    {item.finishDateTime?.toLocaleDateString() ?? ''} {item.finishDateTime?.toLocaleTimeString() ?? ''}
                </td>
                <td class="command-status-col progress-col">
                    {item.progress}
                </td>
            </tr>
        {/each}
    {/if}
    </tbody>
    
    <tfoot>
        <tr>
            <td colspan="7">
                <Paging bind:pageIndex={pageIndex} 
                        bind:pageSize={pageSize} 
                        total={$store.commandStatuses.total} 
                />
            </td>
        </tr>
    </tfoot>
</table>

<style lang="scss">
    .command-statuses {
      width: var(--sf-command-status-width, 100%);
      border-spacing: var(--sf-command-status-border-spacing, 0);
      caption-side: top;
      
      .command-statuses-search-container {
        text-align: var(--sf-command-status-search-alignment, left);
        
        .command-statuses-search {
          padding: var(--sf-command-status-search-padding, initial);
          font-size: var(--sf-command-status-search-font-size, initial);
          width: var(--sf-command-status-search-width, auto);
          border: var(--sf-command-status-search-border, revert);
          border-radius: var(--sf-command-status-search-border-radius, unset);
        }
      }

      thead {
        tr {
          background: var(--sf-command-status-header-background, inherit) !important;
        }
      }
      
      td.command-status-col, th.command-status-col {
        padding: var(--sf-command-status-cell-padding, 1rem 0.25rem);
      }
      
      td.command-status-col {
        color: var(--sf-command-status-cell-color, inherit);
        font-style: var(--sf-command-status-cell-font-style, inherit);
        font-weight: var(--sf-command-status-cell-font-weight, inherit);
        font-family: var(--sf-command-status-cell-font, inherit);
      }

      th.command-status-col {
        color: var(--sf-command-status-header-color, inherit);
        font-style: var(--sf-command-status-header-font-style, inherit);
        font-weight: var(--sf-command-status-header-font-weight, inherit);
        font-family: var(--sf-command-status-header-font, inherit);
      }
      
      .command-status-row {
        background: var(--sf-command-status-background, inherit) !important;
        
        i::after {
          font-style: var(--sf-command-status-icon-font-style, normal) !important;
        }
        
        &:hover {
          background: var(--sf-command-status-hover-background, inherit) !important;
        }

        &-selected {
          font-weight: var(--sf-command-status-selected-font-weight, bold) !important;
          background: var(--sf-command-status-selected-background, inherit) !important;
        }
        
        &-started {
          color: var(--sf-command-status-started-color, inherit);
          font-style: var(--sf-command-status-started-font-style, inherit);
          font-weight: var(--sf-command-status-started-font-weight, inherit);

          i::after{
            font: var(--sf-command-status-started-icon-font, inherit);
            content: var(--sf-command-status-started-icon-content, " ");
          }
        }

        &-processing {
          color: var(--sf-command-status-processing-color, inherit);
          font-style: var(--sf-command-status-processing-font-style, inherit);
          font-weight: var(--sf-command-status-processing-font-weight, inherit);

          i::after{
            font: var(--sf-command-status-processing-icon-font, inherit);
            content: var(--sf-command-status-processing-icon-content, " ");
          }
        }

        &-completed {
          color: var(--sf-command-status-completed-color, green);
          font-style: var(--sf-command-status-completed-font-style, inherit);
          font-weight: var(--sf-command-status-completed-font-weight, inherit);
          
          i::after{
            font: var(--sf-command-status-completed-icon-font, inherit);
            content: var(--sf-command-status-completed-icon-content, " ");
          }
        }
        
        &-errored {
          color: var(--sf-command-status-errored-color, red);
          font-style: var(--sf-command-status-errored-font-style, inherit);
          font-weight: var(--sf-command-status-errored-font-weight, bold);
          
          i::after{ 
            font: var(--sf-command-status-errored-icon-font, inherit);
            content: var(--sf-command-status-errored-icon-content, "!"); 
          }
          
        }
      }

      .icon-col {
        display: var(--sf-command-status-icon-column-display, table-cell);
        width: var(--sf-command-status-icon-column-width, auto);
        text-align: var(--sf-command-status-icon-column-align, var(--sf-command-status-column-align, left));
      }

      .command-col {
        display: var(--sf-command-status-command-column-display, table-cell);
        width: var(--sf-command-status-command-column-width, auto);
        text-align: var(--sf-command-status-command-column-align, var(--sf-command-status-column-align, left));
      }

      .name-col {
        display: var(--sf-command-status-name-column-display, table-cell);
        width: var(--sf-command-status-name-column-width, auto);
        text-align: var(--sf-command-status-name-column-align, var(--sf-command-status-column-align, left));
      }
      .user-col {
        display: var(--sf-command-status-user-column-display, table-cell);
        width: var(--sf-command-status-user-column-width, auto);
        text-align: var(--sf-command-status-user-column-align, var(--sf-command-status-column-align, left));
      }

      .start-col {
        display: var(--sf-command-status-start-column-display, table-cell);
        width: var(--sf-command-status-start-column-width, auto);
        text-align: var(--sf-command-status-start-column-align, var(--sf-command-status-column-align, left));
      }

      .finish-col {
        display: var(--sf-command-status-finish-column-display, table-cell);
        width: var(--sf-command-status-finish-column-width, auto);
        text-align: var(--sf-command-status-finish-column-align, var(--sf-command-status-column-align, left));
      }

      .progress-col {
        display: var(--sf-command-status-progress-column-display, table-cell);
        width: var(--sf-command-status-progress-column-width, auto);
        text-align: var(--sf-command-status-progress-column-align,  var(--sf-command-status-column-align, left));
      }
    }
</style>