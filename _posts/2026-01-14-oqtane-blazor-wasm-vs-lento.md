---
layout: post
title: Visual Studio lento con Oqtane e Blazor WebAssembly: la causa e la soluzione
date: 2026-01-14 12:00:00
description: Annotazione sulla risoluzione della lentezza di Visual Studio con Oqtane e Blazor WASM.
tags: blazor oqtane visual-studio
categories: sviluppo
---

Durante lo sviluppo con Oqtane e Blazor WebAssembly in Visual Studio, ho riscontrato rallentamenti significativi, soprattutto in fase di debug.

La causa era legata al debugger Mono per i progetti .NET 9+.

Per risolvere questa situazione, basta disabilitare questa opzione da:
- **Tools** → **Options** → **Debugging**
- Individuare l’impostazione relativa al debugger Mono per .NET 9 e versioni successive
- Disattivarla

Dopo questa modifica, Visual Studio ha ripreso a funzionare normalmente.

Riferimenti utili:
- [How to reduce slowness while running the WASM sample in Visual Studio](https://help.syncfusion.com/document-processing/pdf/pdf-viewer/blazor/faqs/how-to-reduce-slowness-while-running-the-wasm-sample-in-visual-studio)