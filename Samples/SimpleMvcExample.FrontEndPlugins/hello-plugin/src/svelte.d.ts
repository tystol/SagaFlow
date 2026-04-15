/// <reference types="svelte" />
/// <reference types="vite/client" />

declare module '*.svelte' {
  import { SvelteComponentTyped } from 'svelte';
  export default class SvelteComponentTyped<
    Props = Record<string, any>,
    Events = Record<string, any>,
    Slots = Record<string, any>
  > extends SvelteComponentTyped<Props, Events, Slots> {}
}
