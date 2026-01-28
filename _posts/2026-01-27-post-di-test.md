---
layout: post
title: post di test
date: 2026-01-27 09:00:00
description: post di test
tags: test
categories: architecture
---

Durante lo sviluppo con Oqtane e Blazor WebAssembly in Visual Studio, ho riscontrato rallentamenti significativi, soprattutto in fase di debug.

La causa era legata al debugger Mono per i progetti .NET 9+.

Per risolvere questa situazione, basta disabilitare questa opzione da:
- **Tools** → **Options** → **Debugging**
- Individuare l'impostazione relativa al debugger Mono per .NET 9 e versioni successive
- Disattivarla

Dopo questa modifica, Visual Studio ha ripreso a funzionare normalmente.

Riferimenti utili:
- [How to reduce slowness while running the WASM sample in Visual Studio](https://help.syncfusion.com/document-processing/pdf/pdf-viewer/blazor/faqs/how-to-reduce-slowness-while-running-the-wasm-sample-in-visual-studio)
